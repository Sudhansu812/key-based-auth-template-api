using KeyBasedAuthenticationTemplate_API.Common;
using Microsoft.AspNetCore.Authorization;

namespace KeyBasedAuthenticationTemplate_API.Authorization
{
    public class ReportAccessAuthorizationHandler : AuthorizationHandler<ReportAccessRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ReportAccessRequirement requirement)
        {
            if (context.User.IsApiKeyAuthenticated() || context.User.IsAdmin())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
