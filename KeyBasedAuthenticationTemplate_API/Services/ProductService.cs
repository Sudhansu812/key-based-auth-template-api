using KeyBasedAuthenticationTemplate_API.Common.Exceptions;
using KeyBasedAuthenticationTemplate_API.DTOs.Products;
using KeyBasedAuthenticationTemplate_API.Entities;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<ProductResponse>> GetAccessibleAsync(Guid userId, bool bypassOwnership, CancellationToken cancellationToken = default)
        {
            var products = bypassOwnership
                ? await _productRepository.GetAllAsync(cancellationToken)
                : await _productRepository.GetByOwnerAsync(userId, cancellationToken);

            return products.Select(ToResponse).ToList();
        }

        public async Task<ProductResponse> GetByIdAsync(Guid id, Guid userId, bool bypassOwnership, CancellationToken cancellationToken = default)
        {
            var product = await GetOwnedOrThrowAsync(id, userId, bypassOwnership, cancellationToken);
            return ToResponse(product);
        }

        public async Task<ProductResponse> CreateAsync(Guid ownerUserId, ProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                OwnerUserId = ownerUserId,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product, cancellationToken);
            await _productRepository.SaveChangesAsync(cancellationToken);

            return ToResponse(product);
        }

        public async Task<ProductResponse> UpdateAsync(Guid id, Guid userId, bool bypassOwnership, ProductRequest request, CancellationToken cancellationToken = default)
        {
            var product = await GetOwnedOrThrowAsync(id, userId, bypassOwnership, cancellationToken);

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.UpdatedAtUtc = DateTime.UtcNow;

            await _productRepository.SaveChangesAsync(cancellationToken);

            return ToResponse(product);
        }

        public async Task DeleteAsync(Guid id, Guid userId, bool bypassOwnership, CancellationToken cancellationToken = default)
        {
            var product = await GetOwnedOrThrowAsync(id, userId, bypassOwnership, cancellationToken);
            _productRepository.Remove(product);
            await _productRepository.SaveChangesAsync(cancellationToken);
        }

        private async Task<Product> GetOwnedOrThrowAsync(Guid id, Guid userId, bool bypassOwnership, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("Product not found.");

            if (!bypassOwnership && product.OwnerUserId != userId)
            {
                throw new ForbiddenException("You do not have access to this product.");
            }

            return product;
        }

        private static ProductResponse ToResponse(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            OwnerUserId = product.OwnerUserId,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc
        };
    }
}
