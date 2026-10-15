using KeyBasedAuthenticationTemplate_API.Repository.DapperDB;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Staging;

namespace KeyBasedAuthenticationTemplate_API.Services
{
    public class StagingService : IStagingService
    {
        private readonly IProductDapperRepository _productRepository;
        private readonly IProductRiskStagingRepository _stagingRepository;
        private readonly ILogger<StagingService> _logger;

        public StagingService(
            IProductDapperRepository productRepository,
            IProductRiskStagingRepository stagingRepository,
            ILogger<StagingService> logger)
        {
            _productRepository = productRepository;
            _stagingRepository = stagingRepository;
            _logger = logger;
        }

        public async Task<StagingLoadResponse> LoadProductRiskSummaryAsync(CancellationToken cancellationToken = default)
        {
            var snapshotAtUtc = DateTime.UtcNow;
            long rowsLoaded = 0;

            // PostgreSQL reader -> SQL Server bulk copy, streamed end to end.
            await _productRepository.StreamRiskSummaryAsync(
                snapshotAtUtc,
                async reader => rowsLoaded = await _stagingRepository.ReplaceAsync(reader, cancellationToken),
                cancellationToken);

            _logger.LogInformation("Loaded {RowCount} row(s) into {Table}", rowsLoaded, ProductRiskStagingRepository.TableName);

            return new StagingLoadResponse
            {
                Table = ProductRiskStagingRepository.TableName,
                RowsLoaded = (int)rowsLoaded,
                SnapshotAtUtc = snapshotAtUtc
            };
        }
    }
}
