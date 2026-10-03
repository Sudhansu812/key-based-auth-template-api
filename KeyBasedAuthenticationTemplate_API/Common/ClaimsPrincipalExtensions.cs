using System.Security.Claims;

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
            principal.IsInRole(Entities.UserRole.Admin.ToString());
    }
}
