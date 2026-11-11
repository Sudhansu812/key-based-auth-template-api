using System.Security.Claims;
using System.Text.Encodings.Web;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.Helpers.Interfaces;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_API.Authentication
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly ApiKeySettings _apiKeySettings;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IApiKeyRepository apiKeyRepository,
            IApiKeyGenerator apiKeyGenerator,
            IOptions<ApiKeySettings> apiKeySettings)
            : base(options, logger, encoder)
        {
            _apiKeyRepository = apiKeyRepository;
            _apiKeyGenerator = apiKeyGenerator;
            _apiKeySettings = apiKeySettings.Value;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var headerValues))
            {
                return AuthenticateResult.NoResult();
            }

            var presentedKey = headerValues.ToString();
            if (!_apiKeyGenerator.TryParse(presentedKey, _apiKeySettings.Prefix, out var keyId, out var secret))
            {
                return AuthenticateResult.Fail("Invalid API key format.");
            }

            var apiKey = await _apiKeyRepository.GetByKeyIdWithRolesAsync(keyId);
            if (apiKey is null || !_apiKeyGenerator.VerifyHash(secret, apiKey.KeyHash))
            {
                return AuthenticateResult.Fail("Invalid API key.");
            }

            if (apiKey.RevokedAtUtc is not null)
            {
                return AuthenticateResult.Fail("API key has been revoked.");
            }

            if (apiKey.ExpiresAtUtc is not null && apiKey.ExpiresAtUtc < DateTime.UtcNow)
            {
                return AuthenticateResult.Fail("API key has expired.");
            }

            apiKey.LastUsedAtUtc = DateTime.UtcNow;
            await _apiKeyRepository.SaveChangesAsync();

            // One role claim per role on the key, so [Authorize(Roles = ...)] and the role-based
            // policies work directly. The owner username drives product ownership scoping.
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, apiKey.Id.ToString()),
                new(ClaimTypes.Name, apiKey.Name),
                new(ApiKeyAuthenticationOptions.OwnerUsernameClaimType, apiKey.OwnerUsername)
            };
            claims.AddRange(apiKey.Roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
