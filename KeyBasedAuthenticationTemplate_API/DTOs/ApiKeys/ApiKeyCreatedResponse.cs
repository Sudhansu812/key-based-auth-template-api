namespace KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys
{
    // Returned exactly once, at creation time. The full key (with secret) cannot
    // be retrieved again afterwards - only the hash is persisted.
    public class ApiKeyCreatedResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullKey { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
    }
}
