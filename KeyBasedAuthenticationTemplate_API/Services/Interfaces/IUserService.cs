using KeyBasedAuthenticationTemplate_API.DTOs.Users;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserSummaryResponse> AssignRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken = default);
    }
}
