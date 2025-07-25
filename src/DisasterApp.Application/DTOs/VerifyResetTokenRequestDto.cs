
using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class VerifyResetTokenRequestDto
{
    [Required]
    public string Token { get; set; } = null!;
}

