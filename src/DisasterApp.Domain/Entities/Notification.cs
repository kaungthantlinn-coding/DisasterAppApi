using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisasterApp.Domain.Enums;

namespace DisasterApp.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public DateTime? ReadAt { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }

        public Guid UserId { get; set; }
        public Guid DisasterReportId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual DisasterReport DisasterReport { get; set; } = null!;
    }
}
