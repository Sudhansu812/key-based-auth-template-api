using System.Text;
using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Contracts.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Reports
{
    // Runs every registered report: fetch -> Excel -> save to disk -> one digest email with the
    // generated files. A failing report is logged and skipped so the others still run; any
    // failure additionally triggers an error-notification email.
    public class ReportRunner
    {
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IEnumerable<IReport> _reports;
        private readonly IReportApiClient _apiClient;
        private readonly AutomationOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<ReportRunner> _logger;

        public ReportRunner(
            IEnumerable<IReport> reports,
            IReportApiClient apiClient,
            IOptions<AutomationOptions> options,
            TimeProvider timeProvider,
            ILogger<ReportRunner> logger)
        {
            _reports = reports;
            _apiClient = apiClient;
            _options = options.Value;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        // Returns true when every step succeeded: staging (if requested), every report, and delivery.
        public async Task<bool> RunAsync(RunMode mode, CancellationToken cancellationToken = default)
        {
            var startedAt = _timeProvider.GetUtcNow();
            _logger.LogInformation("Run started: mode {Mode}, {ReportCount} report(s)", mode, _reports.Count());

            var attachments = new List<EmailAttachment>();
            var summaries = new List<string>();
            var failures = new List<string>();

            // The reports follow the staging load, so if staging fails the run stops here.
            if (mode == RunMode.StageAndReports && !await TryStageAsync(failures, cancellationToken))
            {
                await SendErrorNotificationAsync(failures, cancellationToken);
                _logger.LogError("Run aborted: staging failed, reports were not generated");
                return false;
            }

            foreach (var report in _reports)
            {
                try
                {
                    _logger.LogInformation("Generating report {Report}", report.Name);
                    var generated = await report.GenerateAsync(cancellationToken);

                    var fileName = $"{report.Name}_{startedAt:yyyyMMdd_HHmmss}.xlsx";
                    var path = await SaveAsync(fileName, generated.Content, cancellationToken);

                    _logger.LogInformation(
                        "Report {Report} generated: {RowCount} row(s), {SizeBytes} bytes, saved to {Path}",
                        report.Name, generated.RowCount, generated.Content.Length, path);

                    summaries.Add($"{report.Name}: {generated.RowCount} row(s)");
                    attachments.Add(new EmailAttachment
                    {
                        FileName = fileName,
                        ContentType = XlsxContentType,
                        ContentBase64 = Convert.ToBase64String(generated.Content)
                    });
                }
                catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Report {Report} failed", report.Name);
                    failures.Add($"Report '{report.Name}' failed: {ex.Message}");
                }
            }

            if (attachments.Count > 0)
            {
                await TrySendAsync(
                    "report delivery",
                    new SendEmailRequest
                    {
                        To = [.. _options.ReportRecipients],
                        Subject = $"Automated reports - {startedAt:yyyy-MM-dd}",
                        Body = "The following reports were generated:\n" + string.Join("\n", summaries.Select(s => "- " + s)),
                        Attachments = attachments
                    },
                    failures,
                    cancellationToken);
            }

            if (failures.Count > 0)
            {
                await SendErrorNotificationAsync(failures, cancellationToken);
            }

            _logger.LogInformation(
                "Report run finished: {SucceededCount} generated, {FailureCount} failure(s)",
                attachments.Count, failures.Count);

            return failures.Count == 0;
        }

        private async Task<bool> TryStageAsync(List<string> failures, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Loading staging table");
                var result = await _apiClient.LoadProductRiskStagingAsync(cancellationToken);
                _logger.LogInformation(
                    "Staging loaded: {RowsLoaded} row(s) into {Table}, snapshot {SnapshotAtUtc:O}",
                    result.RowsLoaded, result.Table, result.SnapshotAtUtc);
                return true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Staging load failed");
                failures.Add($"Staging load failed: {ex.Message}");
                return false;
            }
        }

        private async Task<string> SaveAsync(string fileName, byte[] content, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_options.ReportOutputDirectory);
            var path = Path.GetFullPath(Path.Combine(_options.ReportOutputDirectory, fileName));
            await File.WriteAllBytesAsync(path, content, cancellationToken);
            return path;
        }

        // Sends the email; if that fails the failure is recorded (and logged) instead of thrown.
        private async Task TrySendAsync(string purpose, SendEmailRequest email, List<string> failures, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _apiClient.SendEmailAsync(email, cancellationToken);
                _logger.LogInformation(
                    "Email sent for {Purpose}: message {MessageId} to {Recipients}",
                    purpose, response.MessageId, email.To);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Email for {Purpose} failed", purpose);
                failures.Add($"Email for {purpose} failed: {ex.Message}");
            }
        }

        private async Task SendErrorNotificationAsync(IReadOnlyList<string> failures, CancellationToken cancellationToken)
        {
            var body = new StringBuilder()
                .AppendLine("The automated report run completed with errors:")
                .AppendLine();
            foreach (var failure in failures)
            {
                body.Append("- ").AppendLine(failure);
            }
            body.AppendLine().Append("See the automation log file for full details.");

            var email = new SendEmailRequest
            {
                To = [.. _options.ErrorNotificationRecipients],
                Subject = $"[ALERT] Report automation failed - {failures.Count} error(s)",
                Body = body.ToString()
            };

            // If the API itself is the problem this will fail too; that is logged but must not
            // mask the original failures, so the (discarded) list is deliberately a throwaway.
            await TrySendAsync("error notification", email, [], cancellationToken);
        }
    }
}
