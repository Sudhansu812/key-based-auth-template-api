using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.Common.Exceptions;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Helpers.Interfaces;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IApiKeyGenerator _apiKeyGenerator;
        private readonly ApiKeySettings _apiKeySettings;

        public ApiKeyService(
            IApiKeyRepository apiKeyRepository,
            IRoleRepository roleRepository,
            IApiKeyGenerator apiKeyGenerator,
            IOptions<ApiKeySettings> apiKeySettings)
        {
            _apiKeyRepository = apiKeyRepository;
            _roleRepository = roleRepository;
            _apiKeyGenerator = apiKeyGenerator;
            _apiKeySettings = apiKeySettings.Value;
        }

        public async Task<ApiKeyCreatedResponse> CreateAsync(CreateApiKeyRequest request, CancellationToken cancellationToken = default)
        {
            var roles = await ResolveRolesAsync(request.Roles, cancellationToken);
            var generated = _apiKeyGenerator.Generate(_apiKeySettings.Prefix);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                OwnerUsername = request.OwnerUsername.Trim(),
                KeyId = generated.KeyId,
                KeyHash = generated.KeyHash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = request.ExpiresInDays.HasValue
                    ? DateTime.UtcNow.AddDays(request.ExpiresInDays.Value)
                    : null,
                Roles = roles
            };

            await _apiKeyRepository.AddAsync(apiKey, cancellationToken);
            await _apiKeyRepository.SaveChangesAsync(cancellationToken);

            return new ApiKeyCreatedResponse
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                OwnerUsername = apiKey.OwnerUsername,
                FullKey = generated.FullKey,
                Roles = RoleNames(apiKey),
                CreatedAtUtc = apiKey.CreatedAtUtc,
                ExpiresAtUtc = apiKey.ExpiresAtUtc
            };
        }

        public async Task<List<ApiKeySummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var keys = await _apiKeyRepository.GetAllWithRolesAsync(cancellationToken);
            return keys.Select(ToSummary).ToList();
        }

        public async Task<ApiKeySummaryResponse> AssignRolesAsync(Guid apiKeyId, List<string> roleNames, CancellationToken cancellationToken = default)
        {
            var apiKey = await _apiKeyRepository.GetByIdWithRolesAsync(apiKeyId, cancellationToken)
                ?? throw new NotFoundException("API key not found.");

            var roles = await ResolveRolesAsync(roleNames, cancellationToken);

            var losesAdmin = HasAdminRole(apiKey) && !roles.Any(r => r.Name == SystemRoleNames.Admin);
            if (losesAdmin)
            {
                await EnsureAnotherAdminExistsAsync(apiKey, "Cannot remove the Admin role from the last active Admin key.", cancellationToken);
            }

            apiKey.Roles.Clear();
            foreach (var role in roles)
            {
                apiKey.Roles.Add(role);
            }

            await _apiKeyRepository.SaveChangesAsync(cancellationToken);

            return ToSummary(apiKey);
        }

        public async Task RevokeAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
        {
            var apiKey = await _apiKeyRepository.GetByIdWithRolesAsync(apiKeyId, cancellationToken)
                ?? throw new NotFoundException("API key not found.");

            if (apiKey.RevokedAtUtc is not null)
            {
                return;
            }

            if (HasAdminRole(apiKey))
            {
                await EnsureAnotherAdminExistsAsync(apiKey, "Cannot revoke the last active Admin key.", cancellationToken);
            }

            apiKey.RevokedAtUtc = DateTime.UtcNow;
            await _apiKeyRepository.SaveChangesAsync(cancellationToken);
        }

        // Without users, an Admin key is the only way to manage keys - never let the last one go.
        private async Task EnsureAnotherAdminExistsAsync(ApiKey apiKey, string message, CancellationToken cancellationToken)
        {
            if (!IsActive(apiKey))
            {
                return;
            }

            var otherAdmins = await _apiKeyRepository.CountActiveWithRoleAsync(SystemRoleNames.Admin, apiKey.Id, cancellationToken);
            if (otherAdmins == 0)
            {
                throw new ConflictException(message);
            }
        }

        private async Task<List<Role>> ResolveRolesAsync(List<string> roleNames, CancellationToken cancellationToken)
        {
            var requested = roleNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requested.Count == 0)
            {
                throw new BadRequestException("At least one role is required.");
            }

            var roles = await _roleRepository.GetByNamesAsync(requested, cancellationToken);
            var missing = requested
                .Where(n => !roles.Any(r => string.Equals(r.Name, n, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (missing.Count > 0)
            {
                throw new BadRequestException($"Unknown role(s): {string.Join(", ", missing)}.");
            }

            return roles;
        }

        private static bool HasAdminRole(ApiKey apiKey) =>
            apiKey.Roles.Any(r => r.Name == SystemRoleNames.Admin);

        private static bool IsActive(ApiKey apiKey) =>
            apiKey.RevokedAtUtc is null && (apiKey.ExpiresAtUtc is null || apiKey.ExpiresAtUtc > DateTime.UtcNow);

        private static List<string> RoleNames(ApiKey apiKey) =>
            apiKey.Roles.Select(r => r.Name).OrderBy(n => n, StringComparer.Ordinal).ToList();

        private static ApiKeySummaryResponse ToSummary(ApiKey k) => new()
        {
            Id = k.Id,
            Name = k.Name,
            OwnerUsername = k.OwnerUsername,
            KeyId = k.KeyId,
            Roles = RoleNames(k),
            CreatedAtUtc = k.CreatedAtUtc,
            ExpiresAtUtc = k.ExpiresAtUtc,
            RevokedAtUtc = k.RevokedAtUtc,
            LastUsedAtUtc = k.LastUsedAtUtc
        };
    }
}
