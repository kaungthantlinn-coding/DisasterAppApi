using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportDisasterReportsToExcelAsync();
        Task<byte[]> ExportDisasterReportsToPdfAsync();
    }
}
