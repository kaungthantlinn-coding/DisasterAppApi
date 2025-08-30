using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IReportExportService
    {
        Task<byte[]> ExportDisasterReportsToExcelAsync();
        Task<byte[]> ExportDisasterReportsToPdfAsync();
    }
}
