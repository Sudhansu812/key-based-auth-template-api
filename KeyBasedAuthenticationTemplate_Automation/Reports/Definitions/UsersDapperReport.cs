using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Users;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Users and roles (SQL Server) through the Dapper report endpoint.
    public class UsersDapperReport : ApiReport<UserSummaryResponse>
    {
        public UsersDapperReport(IReportApiClient apiClient, IExcelReportWriter excelWriter)
            : base(apiClient, excelWriter)
        {
        }

        public override string Name => "users-dapper";
        protected override string SheetName => "Users (Dapper)";
        protected override string Endpoint => "api/reports/dapper/users";
        protected override IReadOnlyList<ExcelColumn<UserSummaryResponse>> Columns => ReportColumns.Users;
    }
}
