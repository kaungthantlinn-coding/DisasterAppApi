using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class GoogleLoginRequestDto
{
    [Required]
    public string IdToken { get; set; } = null!;
    
    public string? DeviceInfo { get; set; }
}