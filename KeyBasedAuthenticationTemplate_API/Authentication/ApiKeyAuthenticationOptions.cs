using Microsoft.AspNetCore.Authentication;

namespace KeyBasedAuthenticationTemplate_API.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string SchemeName = "ApiKey";
        public const string HeaderName = "X-Api-Key";
        public const string AccessLevelClaimType = "access_level";
    }
}
