using KeyBasedAuthenticationTemplate_API.DTOs.Roles;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
        Task<List<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
