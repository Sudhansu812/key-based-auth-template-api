using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Helpers;
using KeyBasedAuthenticationTemplate_API.Helpers.Interfaces;
using KeyBasedAuthenticationTemplate_API.Repository.DapperDB;
using KeyBasedAuthenticationTemplate_API.Repository.DB;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;

namespace KeyBasedAuthenticationTemplate_API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

            // Dapper read path (same databases as the EF Core contexts).
            services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            services.AddScoped<IProductDapperRepository, ProductDapperRepository>();
            services.AddScoped<IApiKeyDapperRepository, ApiKeyDapperRepository>();
            services.AddScoped<IProductRiskStagingRepository, ProductRiskStagingRepository>();

            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IStagingService, StagingService>();
            services.AddScoped<IEmailService, MockEmailService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddSingleton<IApiKeyGenerator, ApiKeyGenerator>();

            return services;
        }
    }
}
