using System.ComponentModel.DataAnnotations;

namespace DisasterApp.Application.DTOs;

public class ValidatePasswordRequestDto
{
    [Required]
    public string Password { get; set; } = null!;
}
