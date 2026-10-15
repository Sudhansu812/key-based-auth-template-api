using Microsoft.AspNetCore.Authorization;

namespace KeyBasedAuthenticationTemplate_API.Authorization
{
    // Gates the report and notification endpoints. Reports return every user/product, so they are
    // limited to service credentials (API keys) and Admins - regular JWT users are excluded.
    public class ReportAccessRequirement : IAuthorizationRequirement
    {
        public const string PolicyName = "ReportAccess";
    }
}
