using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class SupportRequestSupportType
{
    public int SupportRequestId { get; set; }

    public int SupportTypeId { get; set; }

    public virtual SupportRequest SupportRequest { get; set; } = null!;

    public virtual SupportType SupportType { get; set; } = null!;
}