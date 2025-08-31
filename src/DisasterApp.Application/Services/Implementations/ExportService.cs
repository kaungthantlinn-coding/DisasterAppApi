using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using ClosedXML.Excel;
using System.Globalization;//
using System.Text;
using Microsoft.Extensions.Logging;

namespace DisasterApp.Application.Services.Implementations;

public class ExportService : IExportService
{
    private readonly List<string> _availableFields;
    private readonly Dictionary<string, string> _fieldDisplayNames;

    public ExportService()
    {
        _availableFields = InitializeAvailableFields();
        _fieldDisplayNames = InitializeFieldDisplayNames();
    }

    public async Task<byte[]> ExportToCsvAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null)
    {
        var fieldsToExport = fields ?? _availableFields;
        var csv = new StringBuilder();

        // Add header
        var headers = fieldsToExport.Select(f => _fieldDisplayNames.GetValueOrDefault(f, f));
        csv.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));

        // Add data rows
        foreach (var log in data)
        {
            var values = fieldsToExport.Select(field => GetFieldValue(log, field));
            csv.AppendLine(string.Join(",", values.Select(EscapeCsvValue)));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public async Task<byte[]> ExportToExcelAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null)
    {
        var fieldsToExport = fields ?? _availableFields;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Audit Logs");

        // Add headers
        for (int i = 0; i < fieldsToExport.Count; i++)
        {
            var field = fieldsToExport[i];
            var displayName = _fieldDisplayNames.GetValueOrDefault(field, field);
            worksheet.Cell(1, i + 1).Value = displayName;
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        // Add data
        var dataList = data.ToList();
        for (int row = 0; row < dataList.Count; row++)
        {
            var log = dataList[row];
            for (int col = 0; col < fieldsToExport.Count; col++)
            {
                var field = fieldsToExport[col];
                var value = GetFieldValue(log, field);
                
                // Handle different data types
                if (field == "Timestamp" && DateTime.TryParse(value, out var dateValue))
                {
                    worksheet.Cell(row + 2, col + 1).Value = dateValue;
                    worksheet.Cell(row + 2, col + 1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                }
                else
                {
                    worksheet.Cell(row + 2, col + 1).Value = value;
                }
            }
        }

        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        // Apply formatting to header row
        var headerRange = worksheet.Range(1, 1, 1, fieldsToExport.Count);
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportToPdfAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null)
    {
        var fieldsToExport = fields ?? _availableFields;
        var dataList = data.ToList(); // Convert once to avoid multiple enumeration
        
        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        // Add title
        document.Add(new Paragraph("Audit Logs Report")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(14)
            .SetBold());

        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(9)
            .SetMarginBottom(15));

        // Create table with optimized settings
        var table = new Table(fieldsToExport.Count);
        table.SetWidth(UnitValue.CreatePercentValue(100));
        table.SetKeepTogether(false); // Allow table to break across pages

        // Add headers with minimal styling
        foreach (var field in fieldsToExport)
        {
            var displayName = _fieldDisplayNames.GetValueOrDefault(field, field);
            table.AddHeaderCell(new Cell().Add(new Paragraph(displayName).SetBold().SetFontSize(10)));
        }

        // Add data rows with batch processing for better performance
        var batchSize = 100;
        for (int i = 0; i < dataList.Count; i += batchSize)
        {
            var batch = dataList.Skip(i).Take(batchSize);
            foreach (var log in batch)
            {
                foreach (var field in fieldsToExport)
                {
                    var value = GetFieldValue(log, field);
                    var cellValue = string.IsNullOrEmpty(value) ? "-" : (value.Length > 100 ? value.Substring(0, 100) + "..." : value);
                    table.AddCell(new Cell().Add(new Paragraph(cellValue).SetFontSize(8)));
                }
            }
        }

        document.Add(table);
        document.Close();

        return stream.ToArray();
    }

    public List<string> GetAvailableFields()
    {
        return _availableFields.ToList();
    }

    public bool ValidateFields(List<string> fields)
    {
        return fields.All(field => _availableFields.Contains(field, StringComparer.OrdinalIgnoreCase));
    }

    public string GetMimeType(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "csv" => "text/csv",
            "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    public string GetFileExtension(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "csv" => "csv",
            "excel" => "xlsx",
            "pdf" => "pdf",
            _ => "bin"
        };
    }

    private string GetFieldValue(AuditLogDto log, string field)
    {
        return field.ToLowerInvariant() switch
        {
            "id" => log.Id,
            "timestamp" => log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            "action" => log.Action,
            "severity" => log.Severity,
            "userid" => log.User?.Id ?? "",
            "username" => log.User?.Name ?? "",
            "useremail" => log.User?.Email ?? "",
            "details" => log.Details,
            "ipaddress" => log.IpAddress ?? "",
            "useragent" => log.UserAgent ?? "",
            "resource" => log.Resource,
            "metadata" => log.Metadata != null ? string.Join("; ", log.Metadata.Select(kvp => $"{kvp.Key}: {kvp.Value}")) : "",
            _ => ""
        };
    }

    private string EscapeCsvValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private List<string> InitializeAvailableFields()
    {
        return new List<string>
        {
            "Id",
            "Timestamp",
            "Action",
            "Severity",
            "UserId",
            "UserName",
            "UserEmail",
            "Details",
            "IpAddress",
            "UserAgent",
            "Resource",
            "Metadata"
        };
    }

    private Dictionary<string, string> InitializeFieldDisplayNames()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = "ID",
            ["Timestamp"] = "Timestamp",
            ["Action"] = "Action",
            ["Severity"] = "Severity",
            ["UserId"] = "User ID",
            ["UserName"] = "User Name",
            ["UserEmail"] = "User Email",
            ["Details"] = "Details",
            ["IpAddress"] = "IP Address",
            ["UserAgent"] = "User Agent",
            ["Resource"] = "Resource",
            ["Metadata"] = "Metadata"
        };
    }
}
