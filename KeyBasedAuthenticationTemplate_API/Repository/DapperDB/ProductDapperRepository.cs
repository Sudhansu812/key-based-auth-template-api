using System.Data;
using Dapper;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;

namespace KeyBasedAuthenticationTemplate_API.Repository.DapperDB
{
    public class ProductDapperRepository : IProductDapperRepository
    {
        private static readonly string GetAllSql = SqlQueries.Load("Postgres.ProductsAll");
        private static readonly string RiskSummarySql = SqlQueries.Load("Postgres.ProductRiskSummary");

        // "High" cost = price >= HighPrice, "High" stock = stock >= HighStock (see ProductRiskSummary.sql).
        private const decimal HighPriceThreshold = 250m;
        private const int HighStockThreshold = 250;

        private readonly IDbConnectionFactory _connectionFactory;
        private readonly int _commandTimeoutSeconds;

        public ProductDapperRepository(IDbConnectionFactory connectionFactory, IOptions<DatabaseSettings> settings)
        {
            _connectionFactory = connectionFactory;
            _commandTimeoutSeconds = settings.Value.CommandTimeoutSeconds;
        }

        // DAPPER: parameters are described with DynamicParameters so each one carries an explicit
        // DbType (and size / precision / scale where it matters) instead of leaving Dapper to guess
        // from the .NET type.
        public async Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var parameters = new DynamicParameters();
            parameters.Add("MinPrice", NoOpFilterValues.MinPrice, DbType.Decimal, precision: 18, scale: 2);
            parameters.Add("MinStock", NoOpFilterValues.MinStock, DbType.Int32);
            parameters.Add("OwnerUserId", NoOpFilterValues.OwnerUserId, DbType.Guid);
            parameters.Add("NamePattern", NoOpFilterValues.NamePattern, DbType.String, size: 256);
            parameters.Add("CreatedFrom", NoOpFilterValues.CreatedFrom, DbType.DateTime);
            parameters.Add("UpdatedBefore", NoOpFilterValues.UpdatedBefore, DbType.DateTime); // typed NULL
            parameters.Add("InStockOnly", NoOpFilterValues.InStockOnly, DbType.Boolean);

            await using var connection = _connectionFactory.CreateAppConnection();
            var rows = await connection.QueryAsync<ProductResponse>(new CommandDefinition(
                GetAllSql,
                parameters,
                commandTimeout: _commandTimeoutSeconds,
                cancellationToken: cancellationToken));
            return rows.AsList();
        }

        // PLAIN ADO.NET: the reader has to be handed to SqlBulkCopy still open, so a raw command is the
        // natural fit here. Parameters are NpgsqlParameters with an explicit NpgsqlDbType - the most
        // precise way to bind on PostgreSQL (timestamptz vs timestamp, numeric vs double, uuid vs text).
        // The connection and reader stay open for the duration of the consumer, so rows flow straight
        // through without being buffered.
        public async Task StreamRiskSummaryAsync(
            DateTime snapshotAtUtc,
            Func<IDataReader, Task> consumer,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _connectionFactory.CreateAppConnection();
            await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(RiskSummarySql, connection)
            {
                CommandTimeout = _commandTimeoutSeconds
            };

            // Real parameters.
            command.Parameters.Add(new NpgsqlParameter("HighPrice", NpgsqlDbType.Numeric) { Value = HighPriceThreshold });
            command.Parameters.Add(new NpgsqlParameter("HighStock", NpgsqlDbType.Integer) { Value = HighStockThreshold });
            command.Parameters.Add(new NpgsqlParameter("SnapshotAtUtc", NpgsqlDbType.TimestampTz) { Value = snapshotAtUtc });

            // No-op demo parameters (see the "1 = 1 OR (...)" clause in the .sql file).
            command.Parameters.Add(new NpgsqlParameter("MinPrice", NpgsqlDbType.Numeric) { Value = NoOpFilterValues.MinPrice });
            command.Parameters.Add(new NpgsqlParameter("MinStock", NpgsqlDbType.Integer) { Value = NoOpFilterValues.MinStock });
            command.Parameters.Add(new NpgsqlParameter("OwnerUserId", NpgsqlDbType.Uuid) { Value = NoOpFilterValues.OwnerUserId });
            command.Parameters.Add(new NpgsqlParameter("NamePattern", NpgsqlDbType.Text) { Value = NoOpFilterValues.NamePattern });
            command.Parameters.Add(new NpgsqlParameter("CreatedFrom", NpgsqlDbType.TimestampTz) { Value = NoOpFilterValues.CreatedFrom });
            // Typed NULL: the value must be DBNull.Value, and NpgsqlDbType supplies the type.
            command.Parameters.Add(new NpgsqlParameter("UpdatedBefore", NpgsqlDbType.TimestampTz) { Value = DBNull.Value });
            command.Parameters.Add(new NpgsqlParameter("InStockOnly", NpgsqlDbType.Boolean) { Value = NoOpFilterValues.InStockOnly });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await consumer(reader);
        }
    }
}
