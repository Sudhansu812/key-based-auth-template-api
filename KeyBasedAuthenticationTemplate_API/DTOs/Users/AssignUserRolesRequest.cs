using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_API.DTOs.Users
{
    public class AssignUserRolesRequest
    {
        [Required, MinLength(1)]
        public List<Guid> RoleIds { get; set; } = new();
    }
}
