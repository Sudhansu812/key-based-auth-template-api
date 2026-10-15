using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using KeyBasedAuthenticationTemplate_Contracts.Users;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class ReportService : IReportService
    {
        private readonly IProductDapperRepository _productRepository;
        private readonly IUserDapperRepository _userRepository;

        public ReportService(IProductDapperRepository productRepository, IUserDapperRepository userRepository)
        {
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public Task<List<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken = default) =>
            _productRepository.GetAllAsync(cancellationToken);

        public Task<List<UserSummaryResponse>> GetUsersAsync(CancellationToken cancellationToken = default) =>
            _userRepository.GetAllAsync(cancellationToken);
    }
}
