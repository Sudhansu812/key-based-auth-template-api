using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using KeyBasedAuthenticationTemplate_Contracts.Products;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class ReportService : IReportService
    {
        private readonly IProductDapperRepository _productRepository;
        private readonly IApiKeyDapperRepository _apiKeyRepository;

        public ReportService(IProductDapperRepository productRepository, IApiKeyDapperRepository apiKeyRepository)
        {
            _productRepository = productRepository;
            _apiKeyRepository = apiKeyRepository;
        }

        public Task<List<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken = default) =>
            _productRepository.GetAllAsync(cancellationToken);

        public Task<List<ApiKeySummaryResponse>> GetApiKeysAsync(CancellationToken cancellationToken = default) =>
            _apiKeyRepository.GetAllAsync(cancellationToken);
    }
}
