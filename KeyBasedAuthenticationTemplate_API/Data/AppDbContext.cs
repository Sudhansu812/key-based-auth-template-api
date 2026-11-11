using KeyBasedAuthenticationTemplate_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(256);
                entity.Property(p => p.Price).HasColumnType("numeric(18,2)");
                entity.Property(p => p.OwnerUsername).IsRequired().HasMaxLength(64);
                entity.HasIndex(p => p.OwnerUsername);
            });
        }
    }
}
