using System.Data;
using Dapper;
using KeyBasedAuthenticationTemplate_API.Configuration;
using KeyBasedAuthenticationTemplate_API.Data;
using KeyBasedAuthenticationTemplate_API.Repository.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Users;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_API.Repository.DapperDB
{
    public class UserDapperRepository : IUserDapperRepository
    {
        private static readonly string GetAllSql = SqlQueries.Load("SqlServer.UsersWithRoles");

        private readonly IDbConnectionFactory _connectionFactory;
        private readonly int _commandTimeoutSeconds;

        public UserDapperRepository(IDbConnectionFactory connectionFactory, IOptions<DatabaseSettings> settings)
        {
            _connectionFactory = connectionFactory;
            _commandTimeoutSeconds = settings.Value.CommandTimeoutSeconds;
        }

        public async Task<List<UserSummaryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            // DAPPER on SQL Server: DbString controls how a string is sent - IsAnsi = false gives nvarchar
            // (matching the column, so no implicit conversion that can defeat an index) and Length keeps the
            // parameter type stable across calls, so the plan cache isn't polluted with one plan per length.
            var parameters = new DynamicParameters();
            parameters.Add("UserId", NoOpFilterValues.OwnerUserId, DbType.Guid);
            parameters.Add("EmailPattern", new DbString { Value = NoOpFilterValues.EmailPattern, IsAnsi = false, Length = 256 });
            parameters.Add("IsActive", NoOpFilterValues.IsActive, DbType.Boolean);
            parameters.Add("CreatedFrom", NoOpFilterValues.CreatedFrom, DbType.DateTime2);
            parameters.Add("UpdatedBefore", NoOpFilterValues.UpdatedBefore, DbType.DateTime2); // typed NULL

            await using var connection = _connectionFactory.CreateAuthConnection();
            var rows = await connection.QueryAsync<UserRoleRow>(new CommandDefinition(
                GetAllSql,
                parameters,
                commandTimeout: _commandTimeoutSeconds,
                cancellationToken: cancellationToken));

            // Rows arrive ordered by user, so GroupBy preserves the user ordering.
            return rows
                .GroupBy(r => r.Id)
                .Select(group =>
                {
                    var first = group.First();
                    return new UserSummaryResponse
                    {
                        Id = first.Id,
                        Email = first.Email,
                        Username = first.Username,
                        IsActive = first.IsActive,
                        CreatedAtUtc = first.CreatedAtUtc,
                        Roles = group.Where(r => r.RoleName is not null).Select(r => r.RoleName!).ToList()
                    };
                })
                .ToList();
        }

        private sealed class UserRoleRow
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public string? RoleName { get; set; }
        }
    }
}
