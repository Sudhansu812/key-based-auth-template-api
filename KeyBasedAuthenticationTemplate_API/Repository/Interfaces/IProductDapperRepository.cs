using System.Data;
using KeyBasedAuthenticationTemplate_Contracts.Products;

namespace KeyBasedAuthenticationTemplate_API.Repository.Interfaces
{
    public interface IProductDapperRepository
    {
        Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);

        // Streams the product risk grouping (products grouped by cost band x stock band, with a
        // risk level per group) to the consumer as a forward-only reader - nothing is buffered.
        // Columns: SnapshotAtUtc, RiskLevel, CostBand, StockBand, ProductCount, TotalUnits, InventoryValue.
        // The reader (and its connection) are closed as soon as the consumer returns.
        Task StreamRiskSummaryAsync(
            DateTime snapshotAtUtc,
            Func<IDataReader, Task> consumer,
            CancellationToken cancellationToken = default);
    }
}
