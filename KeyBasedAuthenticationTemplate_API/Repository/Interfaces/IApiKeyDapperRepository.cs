using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;

namespace KeyBasedAuthenticationTemplate_API.Repository.Interfaces
{
    public interface IApiKeyDapperRepository
    {
        Task<List<ApiKeySummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
