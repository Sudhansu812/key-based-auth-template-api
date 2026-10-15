namespace KeyBasedAuthenticationTemplate_Automation.Reports.Excel
{
    public interface IExcelReportWriter
    {
        // Returns the bytes of a single-sheet .xlsx workbook.
        byte[] Write<TRow>(string sheetName, IReadOnlyList<TRow> rows, IReadOnlyList<ExcelColumn<TRow>> columns);
    }
}
