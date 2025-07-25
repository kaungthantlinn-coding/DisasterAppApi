using System;
using System.Collections.Generic;

namespace DisasterApp.Domain.Entities;

public partial class Location
{
    public Guid LocationId { get; set; }

    public Guid ReportId { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string Address { get; set; } = null!;

    public string? FormattedAddress { get; set; }



    public string? CoordinatePrecision { get; set; }

    public virtual DisasterReport Report { get; set; } = null!;
}
