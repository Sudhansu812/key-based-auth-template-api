using Microsoft.Data.SqlClient;
using Npgsql;

namespace KeyBasedAuthenticationTemplate_API.Data
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _authConnectionString;
        private readonly string _appConnectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _authConnectionString = configuration.GetConnectionString("AuthDb")
                ?? throw new InvalidOperationException("Connection string 'AuthDb' is not configured.");
            _appConnectionString = configuration.GetConnectionString("AppDb")
                ?? throw new InvalidOperationException("Connection string 'AppDb' is not configured.");
        }

        public SqlConnection CreateAuthConnection() => new(_authConnectionString);

        public NpgsqlConnection CreateAppConnection() => new(_appConnectionString);
    }
}
