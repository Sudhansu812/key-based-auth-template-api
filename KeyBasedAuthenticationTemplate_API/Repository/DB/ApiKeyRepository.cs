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

        public Task<ApiKey?> GetByKeyIdWithRolesAsync(string keyId, CancellationToken cancellationToken = default) =>
            _context.ApiKeys.Include(k => k.Roles).FirstOrDefaultAsync(k => k.KeyId == keyId, cancellationToken);

        public Task<ApiKey?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.ApiKeys.Include(k => k.Roles).FirstOrDefaultAsync(k => k.Id == id, cancellationToken);

        public Task<List<ApiKey>> GetAllWithRolesAsync(CancellationToken cancellationToken = default) =>
            _context.ApiKeys.AsNoTracking()
                .Include(k => k.Roles)
                .OrderBy(k => k.CreatedAtUtc)
                .ThenBy(k => k.Id)
                .ToListAsync(cancellationToken);

        public Task<int> CountActiveWithRoleAsync(string roleName, Guid excludingApiKeyId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return _context.ApiKeys.CountAsync(k =>
                k.Id != excludingApiKeyId
                && k.RevokedAtUtc == null
                && (k.ExpiresAtUtc == null || k.ExpiresAtUtc > now)
                && k.Roles.Any(r => r.Name == roleName),
                cancellationToken);
        }

        public async Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default) =>
            await _context.ApiKeys.AddAsync(apiKey, cancellationToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
