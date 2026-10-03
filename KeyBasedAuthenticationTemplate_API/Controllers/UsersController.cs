using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.DTOs.Users;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = SystemRoleNames.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserSummaryResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:guid}/roles")]
        public async Task<ActionResult<UserSummaryResponse>> AssignRoles(Guid id, AssignUserRolesRequest request, CancellationToken cancellationToken)
        {
            var result = await _userService.AssignRolesAsync(id, request.RoleIds, cancellationToken);
            return Ok(result);
        }
    }
}
