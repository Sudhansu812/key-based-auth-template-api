using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.DTOs.ApiKeys;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    [ApiController]
    [Route("api/apikeys")]
    [Authorize]
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
            var result = await _apiKeyService.CreateAsync(User.GetUserId(), request, cancellationToken);
            return CreatedAtAction(nameof(GetAll), result);
        }

        [HttpGet]
        public async Task<ActionResult<List<ApiKeySummaryResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _apiKeyService.GetForUserAsync(User.GetUserId(), cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
        {
            await _apiKeyService.RevokeAsync(User.GetUserId(), id, cancellationToken);
            return NoContent();
        }
    }
}
