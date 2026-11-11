using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // API keys and their roles (SQL Server) through the EF Core report endpoint.
    public class ApiKeysEfReport : ApiReport<ApiKeySummaryResponse>
    {
        public ApiKeysEfReport(IReportApiClient apiClient, IExcelReportWriter excelWriter, IOptions<AutomationOptions> options)
            : base(apiClient, excelWriter, options)
        {
        }

        public override string Name => "apikeys-ef";
        protected override IReadOnlyList<ExcelColumn<ApiKeySummaryResponse>> Columns => ReportColumns.ApiKeys;
    }
}
