using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class BlacklistUserDto
{
    [Required(ErrorMessage = "Reason is required")]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Reason must be between 1 and 1000 characters")]
    public string Reason { get; set; } = null!;
}

public class BlacklistUserResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public BlacklistUserDataDto Data { get; set; } = null!;
}

public class BlacklistUserDataDto
{
    public Guid UserId { get; set; }
    public DateTime BlacklistedAt { get; set; }
    public string Reason { get; set; } = null!;
    public Guid BlacklistedBy { get; set; }
    public string BlacklistedByName { get; set; } = null!;
}

public class BlacklistHistoryDto
{
    public Guid Id { get; set; }
    public string Reason { get; set; } = null!;
    public UserSummaryDto BlacklistedBy { get; set; } = null!;
    public DateTime BlacklistedAt { get; set; }
    public UserSummaryDto? UnblacklistedBy { get; set; }
    public DateTime? UnblacklistedAt { get; set; }
    public bool IsActive { get; set; }
}

public class UserSummaryDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhotoUrl { get; set; }
}

public class UnblacklistUserDto
{
    [StringLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters")]
    public string? Reason { get; set; }
}

public class UnblacklistUserResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public UnblacklistUserDataDto Data { get; set; } = null!;
}

public class UnblacklistUserDataDto
{
    public Guid UserId { get; set; }
    public DateTime UnblacklistedAt { get; set; }
    public Guid UnblacklistedBy { get; set; }
    public string UnblacklistedByName { get; set; } = null!;
    public string? Reason { get; set; }
}