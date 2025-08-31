namespace DisasterApp.Application.Services.Interfaces;

public interface IPasswordValidationService
{
    PasswordValidationResult ValidatePassword(string password);
    bool IsPasswordStrong(string password);
}

public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();//
    public PasswordStrength Strength { get; set; } = new();
}

public class PasswordStrength
{
    public int Score { get; set; } // 0-4 (weak to strong)
    public bool HasMinLength { get; set; }
    public bool HasUppercase { get; set; }
    public bool HasLowercase { get; set; }
    public bool HasNumber { get; set; }
    public bool HasSpecialChar { get; set; }
    public List<string> Feedback { get; set; } = new();
}
