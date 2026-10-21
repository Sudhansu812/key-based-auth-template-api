using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Contracts.Notifications;
using KeyBasedAuthenticationTemplate_Contracts.Staging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Api
{
    // Typed HttpClient for the API. BaseAddress, timeout and the X-Api-Key header are set
    // where the client is registered (Program.cs), so this class never touches the secret.
    public class ReportApiClient : IReportApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiOptions _options;
        private readonly ILogger<ReportApiClient> _logger;

        public ReportApiClient(HttpClient httpClient, IOptions<AutomationOptions> options, ILogger<ReportApiClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value.Api;
            _logger = logger;
        }

        public async Task<List<T>> GetListAsync<T>(string path, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            using var response = await SendAsync(request, cancellationToken);
            return await ReadAsync<List<T>>(response, request, cancellationToken) ?? [];
        }

        public async Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, _options.Endpoints.Email)
            {
                Content = JsonContent.Create(request)
            };
            using var response = await SendAsync(message, cancellationToken);
            return await ReadAsync<SendEmailResponse>(response, message, cancellationToken)
                ?? throw new ApiCallException($"POST {_options.Endpoints.Email} returned an empty body.", response.StatusCode);
        }

        public async Task<StagingLoadResponse> LoadProductRiskStagingAsync(CancellationToken cancellationToken = default)
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, _options.Endpoints.ProductRiskStaging);
            using var response = await SendAsync(message, cancellationToken);
            return await ReadAsync<StagingLoadResponse>(response, message, cancellationToken)
                ?? throw new ApiCallException($"POST {_options.Endpoints.ProductRiskStaging} returned an empty body.", response.StatusCode);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.ToString() ?? string.Empty;
            var stopwatch = Stopwatch.StartNew();

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "API call {Method} {Path} failed after {ElapsedMs} ms (no response)",
                    request.Method, path, stopwatch.ElapsedMilliseconds);
                throw new ApiCallException($"{request.Method} {path} failed: {ex.Message}", innerException: ex);
            }

            _logger.LogInformation("API call {Method} {Path} -> {StatusCode} in {ElapsedMs} ms",
                request.Method, path, (int)response.StatusCode, stopwatch.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                var detail = await ReadErrorDetailAsync(response, cancellationToken);
                var statusCode = response.StatusCode;
                response.Dispose();
                throw new ApiCallException(
                    $"{request.Method} {path} returned {(int)statusCode} {statusCode}: {detail}", statusCode);
            }

            return response;
        }

        private static async Task<T?> ReadAsync<T>(HttpResponseMessage response, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new ApiCallException(
                    $"{request.Method} {request.RequestUri} returned an unreadable body: {ex.Message}", response.StatusCode, ex);
            }
        }

        // Prefer the ProblemDetails title; fall back to a truncated raw body.
        private async Task<string> ReadErrorDetailAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string body;
            try
            {
                body = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception)
            {
                return "(no response body)";
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return "(no response body)";
            }

            try
            {
                using var document = JsonDocument.Parse(body);
                if (document.RootElement.ValueKind == JsonValueKind.Object
                    && document.RootElement.TryGetProperty("title", out var title)
                    && title.ValueKind == JsonValueKind.String)
                {
                    return title.GetString()!;
                }
            }
            catch (JsonException)
            {
                // Not JSON - fall through to the raw body.
            }

            var max = _options.ErrorDetailMaxLength;
            return body.Length <= max ? body : body[..max] + "...";
        }
    }
}
