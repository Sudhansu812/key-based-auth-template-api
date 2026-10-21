using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Users;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Users and roles (SQL Server) through the Dapper report endpoint.
    public class UsersDapperReport : ApiReport<UserSummaryResponse>
    {
        public UsersDapperReport(IReportApiClient apiClient, IExcelReportWriter excelWriter, IOptions<AutomationOptions> options)
            : base(apiClient, excelWriter, options)
        {
        }

        public override string Name => "users-dapper";
        protected override IReadOnlyList<ExcelColumn<UserSummaryResponse>> Columns => ReportColumns.Users;
    }
}
