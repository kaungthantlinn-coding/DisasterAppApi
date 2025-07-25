
using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}

