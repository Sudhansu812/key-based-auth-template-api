using KeyBasedAuthenticationTemplate_API.DTOs.Products;
using KeyBasedAuthenticationTemplate_Contracts.Products;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetAccessibleAsync(string ownerUsername, bool bypassOwnership, CancellationToken cancellationToken = default);
        Task<ProductResponse> GetByIdAsync(Guid id, string ownerUsername, bool bypassOwnership, CancellationToken cancellationToken = default);
        Task<ProductResponse> CreateAsync(string ownerUsername, ProductRequest request, CancellationToken cancellationToken = default);
        Task<ProductResponse> UpdateAsync(Guid id, string ownerUsername, bool bypassOwnership, ProductRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, string ownerUsername, bool bypassOwnership, CancellationToken cancellationToken = default);
    }
}
