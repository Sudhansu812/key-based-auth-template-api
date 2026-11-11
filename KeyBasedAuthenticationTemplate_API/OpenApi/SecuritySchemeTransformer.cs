using KeyBasedAuthenticationTemplate_API.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace KeyBasedAuthenticationTemplate_API.OpenApi
{
    // Registers the X-Api-Key security scheme so Scalar's UI shows an "Authorize" option for it.
    public class SecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[ApiKeyAuthenticationOptions.SchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = ApiKeyAuthenticationOptions.HeaderName,
                Description = "API key generated via /api/apikeys (or a seeded dev key), sent as the X-Api-Key header."
            };

            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(ApiKeyAuthenticationOptions.SchemeName, document)] = new List<string>()
            });

            return Task.CompletedTask;
        }
    }
}
