using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KeyBasedAuthenticationTemplate_API.Repository.DB
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default) =>
            _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(
                u => u.Username == usernameOrEmail || u.Email == usernameOrEmail,
                cancellationToken);

        public Task<bool> ExistsByEmailOrUsernameAsync(string email, string username, CancellationToken cancellationToken = default) =>
            _context.Users.AnyAsync(u => u.Email == email || u.Username == username, cancellationToken);

        public Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
            _context.Users.AsNoTracking().Include(u => u.Roles).OrderBy(u => u.CreatedAtUtc).ToListAsync(cancellationToken);

        public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
            await _context.Users.AddAsync(user, cancellationToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
