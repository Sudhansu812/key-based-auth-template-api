namespace KeyBasedAuthenticationTemplate_API.Repository.DapperDB
{
    // Values for the "1 = 1 OR (...)" demo conditions in the .sql files. The conditions are always
    // true, so these values can never affect results - they exist to show how each .NET type is
    // bound as a SQL parameter (see the repositories).
    internal static class NoOpFilterValues
    {
        public const decimal MinPrice = 0m;
        public const int MinStock = 0;
        public static readonly Guid OwnerUserId = Guid.Empty;
        public const string NamePattern = "%";
        public const string EmailPattern = "%";
        public const bool InStockOnly = true;
        public const bool IsActive = true;

        // Explicit UTC kind: PostgreSQL timestamptz parameters reject DateTimeKind.Unspecified/Local.
        public static readonly DateTime CreatedFrom = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // A bound NULL needs an explicit type (DbType / NpgsqlDbType / SqlDbType); a bare null has none,
        // and PostgreSQL then fails with "could not determine data type of parameter".
        public static readonly DateTime? UpdatedBefore = null;
    }
}
