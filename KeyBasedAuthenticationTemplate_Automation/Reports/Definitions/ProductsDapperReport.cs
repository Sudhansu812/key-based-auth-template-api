using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Products;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Products (PostgreSQL) through the Dapper report endpoint.
    public class ProductsDapperReport : ApiReport<ProductResponse>
    {
        public ProductsDapperReport(IReportApiClient apiClient, IExcelReportWriter excelWriter)
            : base(apiClient, excelWriter)
        {
        }

        public override string Name => "products-dapper";
        protected override string SheetName => "Products (Dapper)";
        protected override string Endpoint => "api/reports/dapper/products";
        protected override IReadOnlyList<ExcelColumn<ProductResponse>> Columns => ReportColumns.Products;
    }
}
