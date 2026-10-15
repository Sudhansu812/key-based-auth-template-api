using ClosedXML.Excel;

namespace KeyBasedAuthenticationTemplate_Automation.Reports.Excel
{
    public class ClosedXmlReportWriter : IExcelReportWriter
    {
        // Excel limits sheet names to 31 characters.
        private const int MaxSheetNameLength = 31;

        public byte[] Write<TRow>(string sheetName, IReadOnlyList<TRow> rows, IReadOnlyList<ExcelColumn<TRow>> columns)
        {
            using var workbook = new XLWorkbook();
            var safeName = sheetName.Length <= MaxSheetNameLength ? sheetName : sheetName[..MaxSheetNameLength];
            var sheet = workbook.Worksheets.Add(safeName);

            for (var c = 0; c < columns.Count; c++)
            {
                sheet.Cell(1, c + 1).Value = columns[c].Header;
            }

            for (var r = 0; r < rows.Count; r++)
            {
                for (var c = 0; c < columns.Count; c++)
                {
                    var cell = sheet.Cell(r + 2, c + 1);
                    cell.Value = ToCellValue(columns[c].Value(rows[r]));
                    if (columns[c].NumberFormat is { } format)
                    {
                        cell.Style.NumberFormat.Format = format;
                    }
                }
            }

            sheet.ShowGridLines = false;
            sheet.SheetView.FreezeRows(1);

            if (columns.Count > 0)
            {
                // Styling is applied to explicit ranges, not whole rows, so nothing spills past the last column.
                var header = sheet.Range(1, 1, 1, columns.Count);
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Thin border around every populated cell (header + data) and nothing else.
                var table = sheet.Range(1, 1, rows.Count + 1, columns.Count);
                table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                table.SetAutoFilter();
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // Strings are always written as text, so values starting with '=' are never evaluated as formulas.
        private static XLCellValue ToCellValue(object? value) => value switch
        {
            null => Blank.Value,
            string s => s,
            bool b => b,
            DateTime d => d,
            decimal m => m,
            int i => i,
            long l => l,
            double dbl => dbl,
            _ => value.ToString() ?? string.Empty
        };
    }
}
