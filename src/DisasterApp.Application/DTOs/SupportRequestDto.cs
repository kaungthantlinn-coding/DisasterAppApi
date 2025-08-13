using DisasterApp.Domain.Enums;

namespace DisasterApp.Application.DTOs;

public class SupportRequestDto
{
    public int Id { get; set; }
    public Guid ReportId { get; set; }
    public string Description { get; set; } = null!;
    public byte Urgency { get; set; }
    public SupportRequestStatus? Status { get; set; }
    public Guid UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties as DTOs
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? ReportTitle { get; set; }
    public List<SupportTypeDto> SupportTypes { get; set; } = new();
}

public class SupportRequestCreateDto
{
    public Guid ReportId { get; set; }
    public string Description { get; set; } = null!;
    public byte Urgency { get; set; }
    public List<int> SupportTypeIds { get; set; } = new();
}

public class SupportRequestUpdateDto
{
    public string? Description { get; set; }
    public byte? Urgency { get; set; }
    public SupportRequestStatus? Status { get; set; }
    public List<int>? SupportTypeIds { get; set; }
}

public class SupportTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}