using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; } = false;
}