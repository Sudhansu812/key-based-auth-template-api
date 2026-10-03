using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_API.DTOs.Roles
{
    public class CreateRoleRequest
    {
        [Required, MinLength(2), MaxLength(64)]
        public string Name { get; set; } = string.Empty;
    }
}
