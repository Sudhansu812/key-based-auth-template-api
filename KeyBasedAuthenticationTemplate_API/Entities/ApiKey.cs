namespace KeyBasedAuthenticationTemplate_API.Entities
{
    // The only kind of credential in this API. Permissions come from its Roles
    // (many-to-many through ApiKeyRoles); OwnerUsername scopes which Products it can see.
    public class ApiKey
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Free-text owner (not a foreign key - there is no Users table). Products created with
        // this key are stamped with it, and non-Admin keys only see their owner's products.
        public string OwnerUsername { get; set; } = string.Empty;

        // Plaintext, indexed lookup portion of the key (e.g. "kbat_8f3a1b2c").
        public string KeyId { get; set; } = string.Empty;

        // SHA-256 hash of the secret portion of the key. The secret itself is never stored.
        public string KeyHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public DateTime? LastUsedAtUtc { get; set; }

        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
