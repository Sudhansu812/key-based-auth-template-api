using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using KeyBasedAuthenticationTemplate_Contracts.Users;

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
            new("Owner User Id", p => p.OwnerUserId),
            new("Created (UTC)", p => p.CreatedAtUtc, DateTimeFormat),
            new("Updated (UTC)", p => p.UpdatedAtUtc, DateTimeFormat)
        ];

        public static readonly IReadOnlyList<ExcelColumn<UserSummaryResponse>> Users =
        [
            new("Id", u => u.Id),
            new("Username", u => u.Username),
            new("Email", u => u.Email),
            new("Roles", u => string.Join(", ", u.Roles)),
            new("Active", u => u.IsActive),
            new("Created (UTC)", u => u.CreatedAtUtc, DateTimeFormat)
        ];
    }
}
