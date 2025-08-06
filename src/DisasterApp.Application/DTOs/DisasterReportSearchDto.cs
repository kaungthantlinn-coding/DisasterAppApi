using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class DisasterReportSearchDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string Severity { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string DisasterEventName { get; set; } = null!;
        public string LocationAddress { get; set; } = null!;
    }
}
