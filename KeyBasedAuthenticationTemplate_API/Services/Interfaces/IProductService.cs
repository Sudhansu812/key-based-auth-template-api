using KeyBasedAuthenticationTemplate_API.DTOs.Products;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductResponse>> GetAccessibleAsync(Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<ProductResponse> GetByIdAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<ProductResponse> CreateAsync(Guid ownerUserId, ProductRequest request, CancellationToken cancellationToken = default);
        Task<ProductResponse> UpdateAsync(Guid id, Guid userId, bool isAdmin, ProductRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
    }
}
