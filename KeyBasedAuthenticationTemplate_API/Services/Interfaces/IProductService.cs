using KeyBasedAuthenticationTemplate_API.DTOs.Products;
using KeyBasedAuthenticationTemplate_Contracts.Products;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetAccessibleAsync(Guid userId, bool bypassOwnership, CancellationToken cancellationToken = default);
        Task<ProductResponse> GetByIdAsync(Guid id, Guid userId, bool bypassOwnership, CancellationToken cancellationToken = default);
        Task<ProductResponse> CreateAsync(Guid ownerUserId, ProductRequest request, CancellationToken cancellationToken = default);
        Task<ProductResponse> UpdateAsync(Guid id, Guid userId, bool bypassOwnership, ProductRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, Guid userId, bool bypassOwnership, CancellationToken cancellationToken = default);
    }
}
