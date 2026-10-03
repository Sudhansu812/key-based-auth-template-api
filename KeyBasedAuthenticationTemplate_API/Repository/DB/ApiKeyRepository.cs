using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Repository.DB
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly AuthDbContext _context;

        public ApiKeyRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Task<ApiKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default) =>
            _context.ApiKeys.Include(k => k.User).FirstOrDefaultAsync(k => k.KeyId == keyId, cancellationToken);

        public Task<List<ApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            _context.ApiKeys.AsNoTracking()
                .Where(k => k.UserId == userId)
                .OrderByDescending(k => k.CreatedAtUtc)
                .ToListAsync(cancellationToken);

        public Task<ApiKey?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
            _context.ApiKeys.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId, cancellationToken);

        public async Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default) =>
            await _context.ApiKeys.AddAsync(apiKey, cancellationToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
