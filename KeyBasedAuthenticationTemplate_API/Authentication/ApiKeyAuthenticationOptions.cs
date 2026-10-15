using KeyBasedAuthenticationTemplate_Contracts.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace KeyBasedAuthenticationTemplate_API.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string SchemeName = "ApiKey";
        public const string HeaderName = ApiKeyHeader.Name;
        public const string AccessLevelClaimType = "access_level";
    }
}
