
namespace DisasterApp.Application.DTOs;

public class VerifyResetTokenResponseDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = null!;
}

