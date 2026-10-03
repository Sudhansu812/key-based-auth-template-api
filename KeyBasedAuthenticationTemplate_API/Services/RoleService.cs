using KeyBasedAuthenticationTemplate_API.Common.Exceptions;
using KeyBasedAuthenticationTemplate_API.DTOs.Roles;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
        {
            if (await _roleRepository.ExistsByNameAsync(request.Name, cancellationToken))
            {
                throw new ConflictException("A role with that name already exists.");
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                IsSystemRole = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _roleRepository.AddAsync(role, cancellationToken);
            await _roleRepository.SaveChangesAsync(cancellationToken);

            return ToResponse(role);
        }

        public async Task<List<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var roles = await _roleRepository.GetAllAsync(cancellationToken);
            return roles.Select(ToResponse).ToList();
        }

        private static RoleResponse ToResponse(Role role) => new()
        {
            Id = role.Id,
            Name = role.Name,
            IsSystemRole = role.IsSystemRole,
            CreatedAtUtc = role.CreatedAtUtc
        };
    }
}
