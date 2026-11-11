using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Column layouts shared by the EF and Dapper variants of each report - both endpoints
    // return the same DTO, so only the data source differs.
    internal static class ReportColumns
    {
        private const string DateTimeFormat = "yyyy-mm-dd hh:mm:ss";

        public static readonly IReadOnlyList<ExcelColumn<ProductResponse>> Products =
        [
            new("Id", p => p.Id),
            new("Name", p => p.Name),
            new("Description", p => p.Description),
            new("Price", p => p.Price, "#,##0.00"),
            new("Stock", p => p.Stock),
            new("Owner Username", p => p.OwnerUsername),
            new("Created (UTC)", p => p.CreatedAtUtc, DateTimeFormat),
            new("Updated (UTC)", p => p.UpdatedAtUtc, DateTimeFormat)
        ];

        public static readonly IReadOnlyList<ExcelColumn<ApiKeySummaryResponse>> ApiKeys =
        [
            new("Id", k => k.Id),
            new("Name", k => k.Name),
            new("Owner Username", k => k.OwnerUsername),
            new("Key Id", k => k.KeyId),
            new("Roles", k => string.Join(", ", k.Roles)),
            new("Created (UTC)", k => k.CreatedAtUtc, DateTimeFormat),
            new("Expires (UTC)", k => k.ExpiresAtUtc, DateTimeFormat),
            new("Revoked (UTC)", k => k.RevokedAtUtc, DateTimeFormat),
            new("Last Used (UTC)", k => k.LastUsedAtUtc, DateTimeFormat)
        ];
    }
}
