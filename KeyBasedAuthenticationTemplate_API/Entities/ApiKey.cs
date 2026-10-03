namespace KeyBasedAuthenticationTemplate_API.Entities
{
    // Fully standalone credential - intentionally has no link back to a User.
    // Its only permission concept is AccessLevel.
    public class ApiKey
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Plaintext, indexed lookup portion of the key (e.g. "kbat_8f3a1b2c").
        public string KeyId { get; set; } = string.Empty;

        // SHA-256 hash of the secret portion of the key. The secret itself is never stored.
        public string KeyHash { get; set; } = string.Empty;

        public ApiKeyAccessLevel AccessLevel { get; set; } = ApiKeyAccessLevel.ReadOnly;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public DateTime? LastUsedAtUtc { get; set; }
    }
}
