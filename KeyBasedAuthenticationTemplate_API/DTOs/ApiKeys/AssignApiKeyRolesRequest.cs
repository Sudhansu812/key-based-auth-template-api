using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys
{
    // Full replace: the key ends up with exactly this set of roles.
    public class AssignApiKeyRolesRequest
    {
        [Required, MinLength(1)]
        public List<string> Roles { get; set; } = new();
    }
}
