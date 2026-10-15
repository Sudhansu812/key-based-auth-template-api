using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;

namespace KeyBasedAuthenticationTemplate_Automation.Reports
{
    // A report is "rows from one API endpoint, laid out in Excel". Subclasses only say which
    // endpoint, how the rows map to columns, and what to call the report.
    public abstract class ApiReport<TRow> : IReport
    {
        private readonly IReportApiClient _apiClient;
        private readonly IExcelReportWriter _excelWriter;

        protected ApiReport(IReportApiClient apiClient, IExcelReportWriter excelWriter)
        {
            _apiClient = apiClient;
            _excelWriter = excelWriter;
        }

        public abstract string Name { get; }
        protected abstract string SheetName { get; }
        protected abstract string Endpoint { get; }
        protected abstract IReadOnlyList<ExcelColumn<TRow>> Columns { get; }

        public async Task<GeneratedReport> GenerateAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _apiClient.GetListAsync<TRow>(Endpoint, cancellationToken);
            var content = _excelWriter.Write(SheetName, rows, Columns);
            return new GeneratedReport(Name, rows.Count, content);
        }
    }
}
