using System.Data;

namespace KeyBasedAuthenticationTemplate_API.Repository.Interfaces
{
    public interface IProductRiskStagingRepository
    {
        // Replaces the whole contents of stg.ProductRiskSummary with the rows from the reader,
        // atomically (truncate + streamed bulk copy in one transaction). The reader's column names
        // must match the table's columns. Returns the number of rows loaded.
        Task<long> ReplaceAsync(IDataReader source, CancellationToken cancellationToken = default);
    }
}
