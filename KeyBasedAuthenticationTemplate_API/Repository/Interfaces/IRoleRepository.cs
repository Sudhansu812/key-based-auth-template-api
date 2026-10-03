using KeyBasedAuthenticationTemplate_API.Entities;

namespace KeyBasedAuthenticationTemplate_API.Repository.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task AddAsync(Role role, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
