namespace KeyBasedAuthenticationTemplate_API.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Seeded roles (Admin, Reader, Writer, Reporter) that the authorization policies
        // depend on - protected from deletion/renaming by any future role-management endpoint.
        public bool IsSystemRole { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    }
}
