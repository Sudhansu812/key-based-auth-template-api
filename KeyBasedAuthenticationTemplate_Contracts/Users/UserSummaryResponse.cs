namespace KeyBasedAuthenticationTemplate_Contracts.Users
{
    public class UserSummaryResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
