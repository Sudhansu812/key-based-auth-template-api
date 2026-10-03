using Microsoft.AspNetCore.Authorization;

namespace KeyBasedAuthenticationTemplate_API.Authorization
{
    // Blocks ReadOnly API keys from mutating endpoints. Has no effect on
    // JWT-authenticated users (they carry no access_level claim at all).
    public class WriteAccessRequirement : IAuthorizationRequirement
    {
    }
}
