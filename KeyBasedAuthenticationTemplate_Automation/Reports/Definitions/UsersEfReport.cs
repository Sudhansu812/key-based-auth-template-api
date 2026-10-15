using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Users;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Users and roles (SQL Server) through the EF Core report endpoint.
    public class UsersEfReport : ApiReport<UserSummaryResponse>
    {
        public UsersEfReport(IReportApiClient apiClient, IExcelReportWriter excelWriter)
            : base(apiClient, excelWriter)
        {
        }

        public override string Name => "users-ef";
        protected override string SheetName => "Users (EF)";
        protected override string Endpoint => "api/reports/ef/users";
        protected override IReadOnlyList<ExcelColumn<UserSummaryResponse>> Columns => ReportColumns.Users;
    }
}
