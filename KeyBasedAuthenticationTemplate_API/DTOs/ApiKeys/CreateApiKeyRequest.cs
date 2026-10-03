using System.ComponentModel.DataAnnotations;
using KeyBasedAuthenticationTemplate_API.Entities;

namespace KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys
{
    public class CreateApiKeyRequest
    {
        [Required, MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        public ApiKeyAccessLevel AccessLevel { get; set; } = ApiKeyAccessLevel.ReadOnly;

        // Null means the key never expires.
        [Range(1, 3650)]
        public int? ExpiresInDays { get; set; }
    }
}
