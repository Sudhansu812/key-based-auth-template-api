using KeyBasedAuthenticationTemplate_API.Authorization;
using KeyBasedAuthenticationTemplate_API.Common;
using KeyBasedAuthenticationTemplate_API.DTOs.Products;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    // Products are scoped by the calling key's OwnerUsername. Admin keys see and change every
    // owner's products; Reporter keys can read every owner's products (GET /api/products is also
    // the "products-ef" report) but writes stay limited to their own owner.
    [ApiController]
    [Route("api/products")]
    [Authorize(Policy = Policies.ReadAccess)]
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
            var result = await _productService.GetAccessibleAsync(User.GetOwnerUsername(), BypassOwnershipForRead(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _productService.GetByIdAsync(id, User.GetOwnerUsername(), BypassOwnershipForRead(), cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Policies.WriteAccess)]
        public async Task<ActionResult<ProductResponse>> Create(ProductRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.CreateAsync(User.GetOwnerUsername(), request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = Policies.WriteAccess)]
        public async Task<ActionResult<ProductResponse>> Update(Guid id, ProductRequest request, CancellationToken cancellationToken)
        {
            var result = await _productService.UpdateAsync(id, User.GetOwnerUsername(), User.IsAdmin(), request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = Policies.WriteAccess)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _productService.DeleteAsync(id, User.GetOwnerUsername(), User.IsAdmin(), cancellationToken);
            return NoContent();
        }

        private bool BypassOwnershipForRead() => User.IsAdmin() || User.IsInRole(SystemRoleNames.Reporter);
    }
}
