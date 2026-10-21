using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Products (PostgreSQL) through the existing EF Core endpoint.
    public class ProductsEfReport : ApiReport<ProductResponse>
    {
        public ProductsEfReport(IReportApiClient apiClient, IExcelReportWriter excelWriter, IOptions<AutomationOptions> options)
            : base(apiClient, excelWriter, options)
        {
        }

        public override string Name => "products-ef";
        protected override IReadOnlyList<ExcelColumn<ProductResponse>> Columns => ReportColumns.Products;
    }
}
