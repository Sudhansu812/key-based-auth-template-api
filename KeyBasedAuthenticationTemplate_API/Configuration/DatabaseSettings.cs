using System.ComponentModel.DataAnnotations;

namespace KeyBasedAuthenticationTemplate_API.Configuration
{
    // Timeouts for the Dapper / SqlBulkCopy paths (EF Core uses its own provider defaults).
    public class DatabaseSettings
    {
        public const string SectionName = "Database";

        // Applied to every Dapper command (reads and TRUNCATE).
        [Range(1, 3600)]
        public int CommandTimeoutSeconds { get; set; } = 60;

        // Applied to SqlBulkCopy loads.
        [Range(1, 3600)]
        public int BulkCopyTimeoutSeconds { get; set; } = 60;
    }
}
