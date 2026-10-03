namespace KeyBasedAuthenticationTemplate_API.Entities
{
    public class ApiKey
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Name { get; set; } = string.Empty;

        // Plaintext, indexed lookup portion of the key (e.g. "kbat_8f3a1b2c").
        public string KeyId { get; set; } = string.Empty;

        // SHA-256 hash of the secret portion of the key. The secret itself is never stored.
        public string KeyHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public DateTime? LastUsedAtUtc { get; set; }
    }
}
