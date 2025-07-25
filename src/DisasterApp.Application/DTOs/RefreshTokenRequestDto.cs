using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}