namespace KeyBasedAuthenticationTemplate_API.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Seeded roles (Admin, User) that must always exist - protected from
        // deletion/renaming by any future role-management endpoint.
        public bool IsSystemRole { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
