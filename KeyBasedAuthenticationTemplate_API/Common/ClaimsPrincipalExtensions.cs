using System.Security.Claims;
using KeyBasedAuthenticationTemplate_API.Authentication;

namespace KeyBasedAuthenticationTemplate_API.Common
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("The current principal has no NameIdentifier claim.");
            return Guid.Parse(value);
        }

        public static bool IsAdmin(this ClaimsPrincipal principal) =>
            principal.IsInRole(SystemRoleNames.Admin);

        // API keys carry no user identity at all (see ApiKeyAuthenticationHandler) -
        // this is how callers distinguish a key-authenticated request from a real user.
        public static bool IsApiKeyAuthenticated(this ClaimsPrincipal principal) =>
            principal.Identity?.AuthenticationType == ApiKeyAuthenticationOptions.SchemeName;
    }
}
