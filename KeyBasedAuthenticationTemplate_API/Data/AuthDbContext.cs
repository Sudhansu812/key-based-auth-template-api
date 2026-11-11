using KeyBasedAuthenticationTemplate_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(64);
                entity.Property(r => r.Description).HasMaxLength(256);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.ToTable("ApiKeys");
                entity.HasKey(k => k.Id);
                entity.Property(k => k.Name).IsRequired().HasMaxLength(128);
                entity.Property(k => k.OwnerUsername).IsRequired().HasMaxLength(64);
                entity.Property(k => k.KeyId).IsRequired().HasMaxLength(32);
                entity.Property(k => k.KeyHash).IsRequired().HasMaxLength(128);
                entity.HasIndex(k => k.KeyId).IsUnique();
                entity.HasIndex(k => k.OwnerUsername);

                entity.HasMany(k => k.Roles)
                    .WithMany(r => r.ApiKeys)
                    .UsingEntity<Dictionary<string, object>>(
                        "ApiKeyRoles",
                        j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                        j => j.HasOne<ApiKey>().WithMany().HasForeignKey("ApiKeyId"));
            });
        }
    }
}
