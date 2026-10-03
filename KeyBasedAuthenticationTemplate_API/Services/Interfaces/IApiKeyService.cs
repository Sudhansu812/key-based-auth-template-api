using KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IApiKeyService
    {
        Task<ApiKeyCreatedResponse> CreateAsync(CreateApiKeyRequest request, CancellationToken cancellationToken = default);
        Task<List<ApiKeySummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task RevokeAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    }
}
