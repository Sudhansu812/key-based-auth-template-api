using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using KeyBasedAuthenticationTemplate_Contracts.Products;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Definitions
{
    // Products (PostgreSQL) through the existing EF Core endpoint.
    public class ProductsEfReport : ApiReport<ProductResponse>
    {
        public ProductsEfReport(IReportApiClient apiClient, IExcelReportWriter excelWriter)
            : base(apiClient, excelWriter)
        {
        }

        public override string Name => "products-ef";
        protected override string SheetName => "Products (EF)";
        protected override string Endpoint => "api/products";
        protected override IReadOnlyList<ExcelColumn<ProductResponse>> Columns => ReportColumns.Products;
    }
}
