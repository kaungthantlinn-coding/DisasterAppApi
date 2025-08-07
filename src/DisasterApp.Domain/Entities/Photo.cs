using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class Photo
{
    public int Id { get; set; }

    public Guid ReportId { get; set; }

    public string Url { get; set; } = null!;

    public string? Caption { get; set; }

    public string? PublicId { get; set; }  // public_id

    public DateTime? UploadedAt { get; set; }

    public virtual DisasterReport Report { get; set; } = null!;
}
