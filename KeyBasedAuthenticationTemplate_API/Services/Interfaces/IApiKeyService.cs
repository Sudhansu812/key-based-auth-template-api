using KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IApiKeyService
    {
        Task<ApiKeyCreatedResponse> CreateAsync(Guid userId, CreateApiKeyRequest request, CancellationToken cancellationToken = default);
        Task<List<ApiKeySummaryResponse>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task RevokeAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default);
    }
}
