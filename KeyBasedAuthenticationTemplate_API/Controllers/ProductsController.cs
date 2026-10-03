using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.DTOs.Products;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _productService.GetAccessibleAsync(User.GetUserId(), BypassOwnership(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _productService.GetByIdAsync(id, User.GetUserId(), BypassOwnership(), cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "WriteAccess")]
        public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken cancellationToken)
        {
            // API keys have no owning user - products they create have no attributable owner.
            var ownerId = User.IsApiKeyAuthenticated() ? Guid.Empty : User.GetUserId();
            var result = await _productService.CreateAsync(ownerId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "WriteAccess")]
        public async Task<ActionResult<ProductResponse>> Update(Guid id, ProductRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.UpdateAsync(id, User.GetUserId(), BypassOwnership(), request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "WriteAccess")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _productService.DeleteAsync(id, User.GetUserId(), BypassOwnership(), cancellationToken);
            return NoContent();
        }

        // Admins and API keys both get unrestricted (within WriteAccess) access to all
        // products, bypassing the per-owner filter that applies to regular JWT users.
        private bool BypassOwnership() => User.IsAdmin() || User.IsApiKeyAuthenticated();
    }
}
