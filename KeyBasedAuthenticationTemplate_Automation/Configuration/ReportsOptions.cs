namespace KeyBasedAuthenticationTemplate_Automation.Configuration
{
    public class ReportsOptions
    {
        // Relative paths resolve against the working directory; use an absolute path for scheduled runs.
        public string OutputDirectory { get; set; } = string.Empty;

        // .NET date format used in the generated file name: {report}_{timestamp}.xlsx
        public string FileTimestampFormat { get; set; } = string.Empty;

        public string[] Recipients { get; set; } = [];

        // string.Format template; {0} is the run start time (UTC).
        public string EmailSubject { get; set; } = string.Empty;

        // Keyed by report name (e.g. "products-ef"). Every report the app registers needs an entry.
        public Dictionary<string, ReportDefinitionOptions> Definitions { get; set; } = new();
    }

    public class ReportDefinitionOptions
    {
        // A disabled report is skipped entirely.
        public bool Enabled { get; set; } = true;

        // API path relative to Api:BaseUrl.
        public string Endpoint { get; set; } = string.Empty;

        public string SheetName { get; set; } = string.Empty;
    }
}
