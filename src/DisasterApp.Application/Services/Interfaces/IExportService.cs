using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
<<<<<<< HEAD
    public interface IExportService
    {
        Task<byte[]> ExportDisasterReportsToExcelAsync();
        Task<byte[]> ExportDisasterReportsToPdfAsync();
    }
=======
    Task<byte[]> ExportToCsvAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null);
    Task<byte[]> ExportToExcelAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null);
    Task<byte[]> ExportToPdfAsync(IEnumerable<AuditLogDto> data, List<string>? fields = null);
    List<string> GetAvailableFields();//
    bool ValidateFields(List<string> fields);
    string GetMimeType(string format);
    string GetFileExtension(string format);
>>>>>>> ktldev
}
