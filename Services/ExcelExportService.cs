using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DatabaseQueryApp.Services
{
    public class ExcelExportService
    {
        public byte[] ExportToExcel(List<Dictionary<string, object>> data, List<string> columns)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Query Results");
            
            // Add headers
            for (int i = 0; i < columns.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = columns[i];
                worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            
            // Add data
            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < columns.Count; col++)
                {
                    var value = data[row][columns[col]];
                    worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? "";
                }
            }
            
            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}