using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // API keys and their roles (SQL Server) through the Dapper report endpoint.
    public class ApiKeysDapperReport : ApiReport<ApiKeySummaryResponse>
    {
        public ApiKeysDapperReport(IReportApiClient apiClient, IExcelReportWriter excelWriter, IOptions<AutomationOptions> options)
            : base(apiClient, excelWriter, options)
        {
        }

        public override string Name => "apikeys-dapper";
        protected override IReadOnlyList<ExcelColumn<ApiKeySummaryResponse>> Columns => ReportColumns.ApiKeys;
    }
}
