using System.Text;
using KeyBasedAuthenticationTemplate_API.Authentication;
using KeyBasedAuthenticationTemplate_API.Authorization;
using KeyBasedAuthenticationTemplate_API.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace KeyBasedAuthenticationTemplate_API.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public const string PolicySchemeName = "DynamicScheme";

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.Configure<ApiKeySettings>(configuration.GetSection(ApiKeySettings.SectionName));

            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = PolicySchemeName;
                options.DefaultChallengeScheme = PolicySchemeName;
            })
            .AddPolicyScheme(PolicySchemeName, "JWT or API Key", options =>
            {
                options.ForwardDefaultSelector = context =>
                    context.Request.Headers.ContainsKey(ApiKeyAuthenticationOptions.HeaderName)
                        ? ApiKeyAuthenticationOptions.SchemeName
                        : JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.SchemeName, _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("WriteAccess", policy => policy.Requirements.Add(new WriteAccessRequirement()));
            });
            services.AddSingleton<IAuthorizationHandler, WriteAccessAuthorizationHandler>();

            return services;
        }
    }
}
