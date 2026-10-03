using KeyBasedAuthenticationTemplate_API.Entities;

namespace KeyBasedAuthenticationTemplate_API.Repository.Interfaces
{
    public interface IApiKeyRepository
    {
        Task<ApiKey?> GetByKeyIdAsync(string keyId, CancellationToken cancellationToken = default);
        Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ApiKey>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(ApiKey apiKey, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
