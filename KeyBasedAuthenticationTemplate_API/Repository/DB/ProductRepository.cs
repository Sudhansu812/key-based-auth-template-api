using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Repository.DB
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        public Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
            _context.Products.AsNoTracking().OrderByDescending(p => p.CreatedAtUtc).ToListAsync(cancellationToken);

        public Task<List<Product>> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default) =>
            _context.Products.AsNoTracking()
                .Where(p => p.OwnerUserId == ownerUserId)
                .OrderByDescending(p => p.CreatedAtUtc)
                .ToListAsync(cancellationToken);

        public async Task AddAsync(Product product, CancellationToken cancellationToken = default) =>
            await _context.Products.AddAsync(product, cancellationToken);

        public void Remove(Product product) => _context.Products.Remove(product);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
