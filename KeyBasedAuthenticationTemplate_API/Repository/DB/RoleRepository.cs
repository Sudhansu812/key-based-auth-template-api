using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Repository.DB
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AuthDbContext _context;

        public RoleRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
            _context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

        public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default) =>
            _context.Roles.AsNoTracking().OrderBy(r => r.Name).ToListAsync(cancellationToken);

        public Task<List<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
            _context.Roles.Where(r => ids.Contains(r.Id)).ToListAsync(cancellationToken);

        public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
            _context.Roles.AnyAsync(r => r.Name == name, cancellationToken);

        public async Task AddAsync(Role role, CancellationToken cancellationToken = default) =>
            await _context.Roles.AddAsync(role, cancellationToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
