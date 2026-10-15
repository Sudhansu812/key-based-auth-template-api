using KeyBasedAuthenticationTemplate_Contracts.Staging;

namespace KeyBasedAuthenticationTemplate_API.Services.Interfaces
{
    public interface IStagingService
    {
        // Reads the product risk grouping from PostgreSQL and loads it into SQL Server staging.
        Task<StagingLoadResponse> LoadProductRiskSummaryAsync(CancellationToken cancellationToken = default);
    }
}
