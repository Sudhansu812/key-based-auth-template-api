namespace KeyBasedAuthenticationTemplate_API.DTOs.Roles
{
    public class RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
