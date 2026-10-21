using KeyBasedAuthenticationTemplate_Automation.Api;
using KeyBasedAuthenticationTemplate_Automation.Configuration;
using KeyBasedAuthenticationTemplate_Automation.Reports.Excel;
using Microsoft.Extensions.Options;

namespace KeyBasedAuthenticationTemplate_Automation.Reports
{
    // A report is "rows from one API endpoint, laid out in Excel". Subclasses only say what the
    // report is called and how rows map to columns; the endpoint, sheet name and enabled flag come
    // from the report's entry under Automation:Reports:Definitions in appsettings.json.
    public abstract class ApiReport<TRow> : IReport
    {
        private readonly IReportApiClient _apiClient;
        private readonly IExcelReportWriter _excelWriter;
        private readonly ReportDefinitionOptions _definition;

        protected ApiReport(IReportApiClient apiClient, IExcelReportWriter excelWriter, IOptions<AutomationOptions> options)
        {
            _apiClient = apiClient;
            _excelWriter = excelWriter;

            // Fail fast (before any report runs) if a registered report has no configuration.
            _definition = options.Value.Reports.Definitions.TryGetValue(Name, out var definition)
                ? definition
                : throw new InvalidOperationException(
                    $"No configuration for report '{Name}': add it under Automation:Reports:Definitions in appsettings.json.");
        }

        public abstract string Name { get; }
        protected abstract IReadOnlyList<ExcelColumn<TRow>> Columns { get; }

        public bool Enabled => _definition.Enabled;
        protected string SheetName => _definition.SheetName;
        protected string Endpoint => _definition.Endpoint;

        public async Task<GeneratedReport> GenerateAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _apiClient.GetListAsync<TRow>(Endpoint, cancellationToken);
            var content = _excelWriter.Write(SheetName, rows, Columns);
            return new GeneratedReport(Name, rows.Count, content);
        }
    }
}
