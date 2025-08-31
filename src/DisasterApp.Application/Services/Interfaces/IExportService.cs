using DisasterApp.Application.DTOs;

namespace DisasterApp.Application.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportToCsvAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null);
    Task<byte[]> ExportToExcelAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null);
    Task<byte[]> ExportToPdfAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null);
    List<string> GetAvailableFields();
    bool ValidateFields(List<string> fields);
    string GetMimeType(string format);
    string GetFileExtension(string format);
}
//