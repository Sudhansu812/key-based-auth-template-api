using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.DTOs.Roles;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles = SystemRoleNames.Admin)]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
        {
            var result = await _roleService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAll), result);
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _roleService.GetAllAsync(cancellationToken);
            return Ok(result);
        }
    }
}
