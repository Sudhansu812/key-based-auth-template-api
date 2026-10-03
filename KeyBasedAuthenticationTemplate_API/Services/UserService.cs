using KeyBasedAuthenticationTemplate_API.DTOs.Users;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(u => new UserSummaryResponse
            {
                Id = u.Id,
                Email = u.Email,
                Username = u.Username,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CreatedAtUtc = u.CreatedAtUtc
            }).ToList();
        }
    }
}
