using KeyBasedAuthenticationTemplate_API.Authentication;
using KeyBasedAuthenticationTemplate_API.Entities;
using Microsoft.AspNetCore.Authorization;

namespace KeyBasedAuthenticationTemplate_API.Authorization
{
    public class WriteAccessAuthorizationHandler : AuthorizationHandler<WriteAccessRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            WriteAccessRequirement requirement)
        {
            var accessLevelClaim = context.User.FindFirst(ApiKeyAuthenticationOptions.AccessLevelClaimType);

            // No claim at all means this is a JWT-authenticated user, not an API key - unaffected.
            if (accessLevelClaim is null || accessLevelClaim.Value == ApiKeyAccessLevel.ReadWrite.ToString())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
