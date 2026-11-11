using KeyBasedAuthenticationTemplate_API.Authentication;
using KeyBasedAuthenticationTemplate_API.Authorization;
using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.Configuration;

namespace KeyBasedAuthenticationTemplate_API.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiKeySettings>(configuration.GetSection(ApiKeySettings.SectionName));

            // API keys are the only authentication scheme.
            services.AddAuthentication(ApiKeyAuthenticationOptions.SchemeName)
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.SchemeName, _ => { });

            // The handler issues one role claim per role on the key, so plain role policies are enough.
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.ReadAccess, policy =>
                    policy.RequireRole(SystemRoleNames.Reader, SystemRoleNames.Writer, SystemRoleNames.Reporter, SystemRoleNames.Admin));
                options.AddPolicy(Policies.WriteAccess, policy =>
                    policy.RequireRole(SystemRoleNames.Writer, SystemRoleNames.Admin));
                options.AddPolicy(Policies.ReportAccess, policy =>
                    policy.RequireRole(SystemRoleNames.Reporter, SystemRoleNames.Admin));
            });

            return services;
        }
    }
}
