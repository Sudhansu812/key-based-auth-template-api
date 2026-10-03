namespace KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys
{
    // Safe-to-list view of an API key - never includes the secret or its hash.
    public class ApiKeySummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public DateTime? LastUsedAtUtc { get; set; }
    }
}
