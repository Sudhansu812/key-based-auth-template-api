using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    // Key management is restricted to keys holding the Admin role.
    [ApiController]
    [Route("api/apikeys")]
    [Authorize(Roles = SystemRoleNames.Admin)]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeysController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiKeyCreatedResponse>> Create(CreateApiKeyRequest request, CancellationToken cancellationToken)
        {
            var result = await _apiKeyService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAll), result);
        }

        [HttpGet]
        public async Task<ActionResult<List<ApiKeySummaryResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _apiKeyService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:guid}/roles")]
        public async Task<ActionResult<ApiKeySummaryResponse>> AssignRoles(Guid id, AssignApiKeyRolesRequest request, CancellationToken cancellationToken)
        {
            var result = await _apiKeyService.AssignRolesAsync(id, request.Roles, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
        {
            await _apiKeyService.RevokeAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
