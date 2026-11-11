using System.Security.Claims;
using KeyBasedAuthenticationTemplate_API.Authentication;

namespace KeyBasedAuthenticationTemplate_API.Common
{
    // Every authenticated principal is an API key (see ApiKeyAuthenticationHandler).
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetApiKeyId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("The current principal has no NameIdentifier claim.");
            return Guid.Parse(value);
        }

        public static string GetOwnerUsername(this ClaimsPrincipal principal) =>
            principal.FindFirstValue(ApiKeyAuthenticationOptions.OwnerUsernameClaimType)
                ?? throw new InvalidOperationException("The current principal has no owner_username claim.");

        public static bool IsAdmin(this ClaimsPrincipal principal) =>
            principal.IsInRole(SystemRoleNames.Admin);
    }
}
