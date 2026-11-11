using System.Data;
using Dapper;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_API.Repository.DapperDB
{
    public class ApiKeyDapperRepository : IApiKeyDapperRepository
    {
        private static readonly string GetAllSql = SqlQueries.Load("SqlServer.ApiKeysWithRoles");

        private readonly IDbConnectionFactory _connectionFactory;
        private readonly int _commandTimeoutSeconds;

        public ApiKeyDapperRepository(IDbConnectionFactory connectionFactory, IOptions<DatabaseSettings> settings)
        {
            _connectionFactory = connectionFactory;
            _commandTimeoutSeconds = settings.Value.CommandTimeoutSeconds;
        }

        public async Task<List<ApiKeySummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // DAPPER on SQL Server: DbString controls how a string is sent - IsAnsi = false gives nvarchar
            // (matching the column, so no implicit conversion that can defeat an index) and Length keeps the
            // parameter type stable across calls, so the plan cache isn't polluted with one plan per length.
            var parameters = new DynamicParameters();
            parameters.Add("ApiKeyId", NoOpFilterValues.ApiKeyId, DbType.Guid);
            parameters.Add("OwnerUsernamePattern", new DbString { Value = NoOpFilterValues.OwnerUsernamePattern, IsAnsi = false, Length = 64 });
            parameters.Add("KeyIdPattern", new DbString { Value = NoOpFilterValues.KeyIdPattern, IsAnsi = false, Length = 32 });
            parameters.Add("CreatedFrom", NoOpFilterValues.CreatedFrom, DbType.DateTime2);
            parameters.Add("RevokedBefore", NoOpFilterValues.UpdatedBefore, DbType.DateTime2); // typed NULL

            await using var connection = _connectionFactory.CreateAuthConnection();
            var rows = await connection.QueryAsync<ApiKeyRoleRow>(new CommandDefinition(
                GetAllSql,
                parameters,
                commandTimeout: _commandTimeoutSeconds,
                cancellationToken: cancellationToken));

            // Rows arrive ordered by key, so GroupBy preserves the key ordering.
            return rows
                .GroupBy(r => r.Id)
                .Select(group =>
                {
                    var first = group.First();
                    return new ApiKeySummaryResponse
                    {
                        Id = first.Id,
                        Name = first.Name,
                        OwnerUsername = first.OwnerUsername,
                        KeyId = first.KeyId,
                        Roles = group.Where(r => r.RoleName is not null).Select(r => r.RoleName!).ToList(),
                        CreatedAtUtc = first.CreatedAtUtc,
                        ExpiresAtUtc = first.ExpiresAtUtc,
                        RevokedAtUtc = first.RevokedAtUtc,
                        LastUsedAtUtc = first.LastUsedAtUtc
                    };
                })
                .ToList();
        }

        private sealed class ApiKeyRoleRow
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string OwnerUsername { get; set; } = string.Empty;
            public string KeyId { get; set; } = string.Empty;
            public DateTime CreatedAtUtc { get; set; }
            public DateTime? ExpiresAtUtc { get; set; }
            public DateTime? RevokedAtUtc { get; set; }
            public DateTime? LastUsedAtUtc { get; set; }
            public string? RoleName { get; set; }
        }
    }
}
