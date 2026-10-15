namespace KeyBasedAuthenticationTemplate_Automation.Reports.Excel
{
    public record ExcelColumn<TRow>(string Header, Func<TRow, object?> Value, string? NumberFormat = null);
}
