using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Users;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Users and roles (SQL Server) through the EF Core report endpoint.
    public class UsersEfReport : ApiReport<UserSummaryResponse>
    {
        public UsersEfReport(IReportApiClient apiClient, IExcelReportWriter excelWriter, IOptions<AutomationOptions> options)
            : base(apiClient, excelWriter, options)
        {
        }

        public override string Name => "users-ef";
        protected override IReadOnlyList<ExcelColumn<UserSummaryResponse>> Columns => ReportColumns.Users;
    }
}
