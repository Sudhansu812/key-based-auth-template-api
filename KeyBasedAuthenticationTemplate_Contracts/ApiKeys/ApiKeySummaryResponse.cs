namespace KeyBasedAuthenticationTemplate_Contracts.ApiKeys
{
    // Safe-to-list view of an API key - never includes the secret or its hash.
    // Returned by GET /api/apikeys and by the /api/reports/*/apikeys report endpoints.
    public class ApiKeySummaryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OwnerUsername { get; set; } = string.Empty;
        public string KeyId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public DateTime? LastUsedAtUtc { get; set; }
    }
}
