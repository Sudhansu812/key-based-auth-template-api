using KeyBasedAuthenticationTemplate_API.Common.Exceptions;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Helpers.Interfaces;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly ApiKeySettings _apiKeySettings;

        public ApiKeyService(
            IApiKeyRepository apiKeyRepository,
            IApiKeyGenerator apiKeyGenerator,
            IOptions<ApiKeySettings> apiKeySettings)
        {
            _apiKeyRepository = apiKeyRepository;
            _apiKeyGenerator = apiKeyGenerator;
            _apiKeySettings = apiKeySettings.Value;
        }

        public async Task<ApiKeyCreatedResponse> CreateAsync(Guid userId, CreateApiKeyRequest request, CancellationToken cancellationToken = default)
        {
            var generated = _apiKeyGenerator.Generate(_apiKeySettings.Prefix);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                KeyId = generated.KeyId,
                KeyHash = generated.KeyHash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = request.ExpiresInDays.HasValue
                    ? DateTime.UtcNow.AddDays(request.ExpiresInDays.Value)
                    : null
            };

            await _apiKeyRepository.AddAsync(apiKey, cancellationToken);
            await _apiKeyRepository.SaveChangesAsync(cancellationToken);

            return new ApiKeyCreatedResponse
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                FullKey = generated.FullKey,
                CreatedAtUtc = apiKey.CreatedAtUtc,
                ExpiresAtUtc = apiKey.ExpiresAtUtc
            };
        }

        public async Task<List<ApiKeySummaryResponse>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var keys = await _apiKeyRepository.GetByUserIdAsync(userId, cancellationToken);
            return keys.Select(k => new ApiKeySummaryResponse
            {
                Id = k.Id,
                Name = k.Name,
                KeyId = k.KeyId,
                CreatedAtUtc = k.CreatedAtUtc,
                ExpiresAtUtc = k.ExpiresAtUtc,
                RevokedAtUtc = k.RevokedAtUtc,
                LastUsedAtUtc = k.LastUsedAtUtc
            }).ToList();
        }

        public async Task RevokeAsync(Guid userId, Guid apiKeyId, CancellationToken cancellationToken = default)
        {
            var apiKey = await _apiKeyRepository.GetByIdAndUserIdAsync(apiKeyId, userId, cancellationToken)
                ?? throw new NotFoundException("API key not found.");

            if (apiKey.RevokedAtUtc is null)
            {
                apiKey.RevokedAtUtc = DateTime.UtcNow;
                await _apiKeyRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
