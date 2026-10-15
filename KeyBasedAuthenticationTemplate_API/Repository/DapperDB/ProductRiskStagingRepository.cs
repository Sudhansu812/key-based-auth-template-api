using System.Data;
using Dapper;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_API.Repository.DapperDB
{
    // Dapper has no bulk insert of its own, so the load uses SqlBulkCopy; Dapper runs the TRUNCATE.
    // The stg.ProductRiskSummary table is created by script, not by EF migrations.
    public class ProductRiskStagingRepository : IProductRiskStagingRepository
    {
        public const string TableName = "stg.ProductRiskSummary";

        private static readonly string TruncateSql = SqlQueries.Load("SqlServer.TruncateProductRiskSummary");

        private const int BatchSize = 10_000;

        // Source reader column -> destination column (same names).
        private static readonly string[] Columns =
        [
            "SnapshotAtUtc", "RiskLevel", "CostBand", "StockBand", "ProductCount", "TotalUnits", "InventoryValue"
        ];

        private readonly IDbConnectionFactory _connectionFactory;
        private readonly DatabaseSettings _settings;

        public ProductRiskStagingRepository(IDbConnectionFactory connectionFactory, IOptions<DatabaseSettings> settings)
        {
            _connectionFactory = connectionFactory;
            _settings = settings.Value;
        }

        public async Task<long> ReplaceAsync(IDataReader source, CancellationToken cancellationToken = default)
        {
            await using var connection = _connectionFactory.CreateAuthConnection();
            await connection.OpenAsync(cancellationToken);

            // Disposing the transaction without committing rolls it back, so a failure part-way
            // through the stream leaves the previous contents of the table untouched.
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(
                TruncateSql,
                transaction: transaction,
                commandTimeout: _settings.CommandTimeoutSeconds,
                cancellationToken: cancellationToken));

            // TableLock: the staging table is exclusively ours for the load, and a table lock allows
            // minimal logging. EnableStreaming makes SqlBulkCopy read the source as it goes.
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction)
            {
                DestinationTableName = TableName,
                BatchSize = BatchSize,
                EnableStreaming = true,
                BulkCopyTimeout = _settings.BulkCopyTimeoutSeconds
            };

            foreach (var column in Columns)
            {
                bulkCopy.ColumnMappings.Add(column, column);
            }

            await bulkCopy.WriteToServerAsync(source, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return bulkCopy.RowsCopied;
        }
    }
}
