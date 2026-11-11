using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using KeyBasedAuthenticationTemplate_Contracts.Products;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    // Dapper-backed report reads: the same data as the EF Core endpoints, via raw SQL.
    public interface IReportService
    {
        Task<List<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken = default);
        Task<List<ApiKeySummaryResponse>> GetApiKeysAsync(CancellationToken cancellationToken = default);
    }
}
