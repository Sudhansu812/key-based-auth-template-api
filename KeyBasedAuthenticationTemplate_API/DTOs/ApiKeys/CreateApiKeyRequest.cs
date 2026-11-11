using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys
{
    public class CreateApiKeyRequest
    {
        [Required, MaxLength(128)]
        public string Name { get; set; } = string.Empty;

        // Products created with this key are owned by this username, and non-Admin keys
        // only see products with the same owner.
        [Required, MinLength(2), MaxLength(64)]
        public string OwnerUsername { get; set; } = string.Empty;

        // Role names, e.g. ["Reader", "Writer"]. Every name must exist in the Roles table.
        [Required, MinLength(1)]
        public List<string> Roles { get; set; } = new();

        // Null means the key never expires.
        [Range(1, 3650)]
        public int? ExpiresInDays { get; set; }
    }
}
