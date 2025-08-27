using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Domain.Enums
{
    public enum UrgencyLevel : byte
    {
        Immediate = 1,   // "immediate"
        Within24h = 2,   // "within_24h"
        WithinWeek = 3,  // "within_week"
        NonUrgent = 4    // "non_urgent"
    }
}
