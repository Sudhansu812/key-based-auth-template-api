using KeyBasedAuthenticationTemplate_Contracts.Users;

namespace KeyBasedAuthenticationTemplate_API.Repository.Interfaces
{
    public interface IUserDapperRepository
    {
        Task<List<UserSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
