using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Helpers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_API.Data.Seed
{
    // Development-only convenience seeding so the template is immediately testable after first run.
    // Idempotent: every insert checks for an existing row first, so re-running never duplicates data.
    // To reload everything from scratch, drop both databases (reset-dev-db.ps1) and start the API again.
    public static class DbInitializer
    {
        // Fixed, publicly known dev keys - they only ever exist in a Development database.
        // Full key = "{ApiKey:Prefix}_{KeyId}.{Secret}", e.g. "kbat_devadmin.dev-admin-secret-change-me".
        public static readonly IReadOnlyList<SeedApiKey> SeedApiKeys =
        [
            new("Dev admin key", "admin", "devadmin", "dev-admin-secret-change-me", [SystemRoleNames.Admin]),
            new("Dev user key", "user", "devuser", "dev-user-secret-change-me", [SystemRoleNames.Reader, SystemRoleNames.Writer]),
            new("Dev user read-only key", "user", "devreader", "dev-reader-secret-change-me", [SystemRoleNames.Reader]),
            new("Dev automation key", "automation", "devautomation", "dev-automation-secret-change-me", [SystemRoleNames.Reporter, SystemRoleNames.Writer])
        ];

        private static readonly (string Name, string Description)[] SystemRoles =
        [
            (SystemRoleNames.Admin, "Manages API keys and roles; full access to every owner's products and all endpoints."),
            (SystemRoleNames.Reader, "Reads the products of its own owner."),
            (SystemRoleNames.Writer, "Creates, updates and deletes the products of its own owner; sends notifications and loads staging (with Reporter)."),
            (SystemRoleNames.Reporter, "Report endpoints, notifications and staging; reads every owner's products.")
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var authContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var apiKeyGenerator = scope.ServiceProvider.GetRequiredService<IApiKeyGenerator>();
            var prefix = scope.ServiceProvider.GetRequiredService<IOptions<ApiKeySettings>>().Value.Prefix;
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));

            await authContext.Database.MigrateAsync();
            await appContext.Database.MigrateAsync();

            // stg.* tables live outside EF migrations.
            await authContext.Database.ExecuteSqlRawAsync(SqlQueries.Load("SqlServer.CreateStagingTables"));

            await SeedRolesAsync(authContext);
            await SeedApiKeysAsync(authContext, apiKeyGenerator, prefix, logger);
            await SeedProductsAsync(appContext);
        }

        private static async Task SeedRolesAsync(AuthDbContext authContext)
        {
            foreach (var (name, description) in SystemRoles)
            {
                if (!await authContext.Roles.AnyAsync(r => r.Name == name))
                {
                    authContext.Roles.Add(new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        Description = description,
                        IsSystemRole = true,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                }
            }

            await authContext.SaveChangesAsync();
        }

        private static async Task SeedApiKeysAsync(AuthDbContext authContext, IApiKeyGenerator apiKeyGenerator, string prefix, ILogger logger)
        {
            var roles = await authContext.Roles.ToListAsync();

            foreach (var seed in SeedApiKeys)
            {
                if (await authContext.ApiKeys.AnyAsync(k => k.KeyId == seed.KeyId))
                {
                    continue;
                }

                authContext.ApiKeys.Add(new ApiKey
                {
                    Id = Guid.NewGuid(),
                    Name = seed.Name,
                    OwnerUsername = seed.OwnerUsername,
                    KeyId = seed.KeyId,
                    KeyHash = apiKeyGenerator.ComputeHash(seed.Secret),
                    CreatedAtUtc = DateTime.UtcNow,
                    Roles = roles.Where(r => seed.Roles.Contains(r.Name)).ToList()
                });

                logger.LogInformation("Seeded dev API key {Name} ({Roles}): {FullKey}",
                    seed.Name, string.Join(", ", seed.Roles), seed.FullKey(prefix));
            }

            await authContext.SaveChangesAsync();
        }

        private static async Task SeedProductsAsync(AppDbContext appContext)
        {
            if (await appContext.Products.AnyAsync())
            {
                return;
            }

            var now = DateTime.UtcNow;
            appContext.Products.AddRange(
                new Product { Id = Guid.NewGuid(), Name = "Wireless Mouse", Description = "Ergonomic 2.4GHz wireless mouse", Price = 24.99m, Stock = 150, OwnerUsername = "user", CreatedAtUtc = now },
                new Product { Id = Guid.NewGuid(), Name = "Mechanical Keyboard", Description = "Hot-swappable mechanical keyboard", Price = 89.99m, Stock = 60, OwnerUsername = "user", CreatedAtUtc = now },
                new Product { Id = Guid.NewGuid(), Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI", Price = 34.50m, Stock = 200, OwnerUsername = "user", CreatedAtUtc = now },
                new Product { Id = Guid.NewGuid(), Name = "27\" Monitor", Description = "QHD IPS monitor, 144Hz", Price = 299.00m, Stock = 25, OwnerUsername = "user", CreatedAtUtc = now },
                new Product { Id = Guid.NewGuid(), Name = "Webcam 1080p", Description = "Autofocus webcam with privacy shutter", Price = 49.99m, Stock = 80, OwnerUsername = "user", CreatedAtUtc = now },
                new Product { Id = Guid.NewGuid(), Name = "Standing Desk", Description = "Electric sit-stand desk, 160x80", Price = 549.00m, Stock = 300, OwnerUsername = "admin", CreatedAtUtc = now },
                new Product { Id = Guid.NewGuid(), Name = "Office Chair", Description = "Mesh ergonomic chair with lumbar support", Price = 279.00m, Stock = 40, OwnerUsername = "admin", CreatedAtUtc = now }
            );
            await appContext.SaveChangesAsync();
        }

        public sealed record SeedApiKey(string Name, string OwnerUsername, string KeyId, string Secret, string[] Roles)
        {
            public string FullKey(string prefix) => $"{prefix}_{KeyId}.{Secret}";
        }
    }
}
