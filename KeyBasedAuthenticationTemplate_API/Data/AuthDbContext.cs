using KeyBasedAuthenticationTemplate_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(64);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();

                entity.HasMany(u => u.Roles)
                    .WithMany(r => r.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "UserRoles",
                        j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                        j => j.HasOne<User>().WithMany().HasForeignKey("UserId"));
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(64);
                entity.HasIndex(r => r.Name).IsUnique();
            });

            // Standalone credential - no relationship to Users at all.
            modelBuilder.Entity<ApiKey>(entity =>
            {
                entity.ToTable("ApiKeys");
                entity.HasKey(k => k.Id);
                entity.Property(k => k.Name).IsRequired().HasMaxLength(128);
                entity.Property(k => k.KeyId).IsRequired().HasMaxLength(32);
                entity.Property(k => k.KeyHash).IsRequired().HasMaxLength(128);
                entity.Property(k => k.AccessLevel).HasConversion<string>().HasMaxLength(32);
                entity.HasIndex(k => k.KeyId).IsUnique();
            });
        }
    }
}
