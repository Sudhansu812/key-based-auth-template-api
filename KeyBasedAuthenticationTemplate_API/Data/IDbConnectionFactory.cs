using Microsoft.Data.SqlClient;
using Npgsql;

namespace KeyBasedAuthenticationTemplate_API.Data
{
    // Raw ADO.NET connections for the Dapper-based paths. They target the same
    // databases (and use the same connection strings) as the EF Core contexts.
    // Provider-specific types are returned so callers can use SqlBulkCopy etc.
    public interface IDbConnectionFactory
    {
        // SQL Server - Users, Roles, UserRoles, ApiKeys, stg.*.
        SqlConnection CreateAuthConnection();

        // PostgreSQL - Products.
        NpgsqlConnection CreateAppConnection();
    }
}
