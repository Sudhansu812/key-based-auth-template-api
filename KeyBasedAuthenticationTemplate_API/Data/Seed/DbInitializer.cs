using KeyBasedAuthenticationTemplate_API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Data.Seed
{
    // Development-only convenience seeding so the template is immediately
    // testable after first run, without manual registration.
    public static class DbInitializer
    {
        public const string SeedAdminPassword = "Admin123!";
        public const string SeedUserPassword = "User123!";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var authContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            await authContext.Database.MigrateAsync();
            await appContext.Database.MigrateAsync();

            var admin = await authContext.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin is null)
            {
                admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@example.com",
                    Username = "admin",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
                admin.PasswordHash = passwordHasher.HashPassword(admin, SeedAdminPassword);
                authContext.Users.Add(admin);
            }

            var regularUser = await authContext.Users.FirstOrDefaultAsync(u => u.Username == "user");
            if (regularUser is null)
            {
                regularUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "user@example.com",
                    Username = "user",
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
                regularUser.PasswordHash = passwordHasher.HashPassword(regularUser, SeedUserPassword);
                authContext.Users.Add(regularUser);
            }

            await authContext.SaveChangesAsync();

            if (!await appContext.Products.AnyAsync())
            {
                appContext.Products.AddRange(
                    new Product { Id = Guid.NewGuid(), Name = "Wireless Mouse", Description = "Ergonomic 2.4GHz wireless mouse", Price = 24.99m, Stock = 150, OwnerUserId = regularUser.Id, CreatedAtUtc = DateTime.UtcNow },
                    new Product { Id = Guid.NewGuid(), Name = "Mechanical Keyboard", Description = "Hot-swappable mechanical keyboard", Price = 89.99m, Stock = 60, OwnerUserId = regularUser.Id, CreatedAtUtc = DateTime.UtcNow },
                    new Product { Id = Guid.NewGuid(), Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI", Price = 34.50m, Stock = 200, OwnerUserId = regularUser.Id, CreatedAtUtc = DateTime.UtcNow },
                    new Product { Id = Guid.NewGuid(), Name = "27\" Monitor", Description = "QHD IPS monitor, 144Hz", Price = 299.00m, Stock = 25, OwnerUserId = regularUser.Id, CreatedAtUtc = DateTime.UtcNow },
                    new Product { Id = Guid.NewGuid(), Name = "Webcam 1080p", Description = "Autofocus webcam with privacy shutter", Price = 49.99m, Stock = 80, OwnerUserId = regularUser.Id, CreatedAtUtc = DateTime.UtcNow }
                );
                await appContext.SaveChangesAsync();
            }
        }
    }
}
