namespace KeyBasedAuthenticationTemplate_Automation.Reports
{
    public interface IReport
    {
        // Short, file-name-safe identifier, e.g. "products-ef".
        string Name { get; }

        // From configuration; a disabled report is skipped.
        bool Enabled { get; }

        Task<GeneratedReport> GenerateAsync(CancellationToken cancellationToken = default);
    }

    public record GeneratedReport(string Name, int RowCount, byte[] Content);
}
