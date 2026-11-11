using KeyBasedAuthenticationTemplate_API.Entities;

namespace KeyBasedAuthenticationTemplate_API.Repository.Interfaces
{
    public interface IApiKeyRepository
    {
        // Tracked, with Roles - used by authentication (which updates LastUsedAtUtc).
        Task<ApiKey?> GetByKeyIdWithRolesAsync(string keyId, CancellationToken cancellationToken = default);
        Task<ApiKey?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ApiKey>> GetAllWithRolesAsync(CancellationToken cancellationToken = default);

        // Active (not revoked, not expired) keys holding the given role, excluding one key.
        Task<int> CountActiveWithRoleAsync(string roleName, Guid excludingApiKeyId, CancellationToken cancellationToken = default);

        Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
