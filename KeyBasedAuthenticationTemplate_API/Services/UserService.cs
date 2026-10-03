using KeyBasedAuthenticationTemplate_API.Common.Exceptions;
using KeyBasedAuthenticationTemplate_API.DTOs.Users;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<List<UserSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(ToResponse).ToList();
        }

        public async Task<UserSummaryResponse> AssignRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("User not found.");

            var roles = await _roleRepository.GetByIdsAsync(roleIds, cancellationToken);
            if (roles.Count != roleIds.Distinct().Count())
            {
                throw new NotFoundException("One or more roles not found.");
            }

            user.Roles.Clear();
            foreach (var role in roles)
            {
                user.Roles.Add(role);
            }

            await _userRepository.SaveChangesAsync(cancellationToken);

            return ToResponse(user);
        }

        private static UserSummaryResponse ToResponse(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
