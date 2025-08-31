using DisasterApp.Application.Services.Interfaces;
using System.Text.RegularExpressions;

namespace DisasterApp.Application.Services.Implementations;

public class PasswordValidationService : IPasswordValidationService//
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;

    public PasswordValidationResult ValidatePassword(string password)
    {
        var result = new PasswordValidationResult();
        var strength = new PasswordStrength();
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required");
            result.IsValid = false;
            result.Errors = errors;
            result.Strength = strength;
            return result;
        }

        // Check minimum length
        strength.HasMinLength = password.Length >= MinPasswordLength;
        if (!strength.HasMinLength)
        {
            errors.Add($"Password must be at least {MinPasswordLength} characters long");
        }

        // Check maximum length
        if (password.Length > MaxPasswordLength)
        {
            errors.Add($"Password must not exceed {MaxPasswordLength} characters");
        }

        // Check for uppercase letter
        strength.HasUppercase = Regex.IsMatch(password, @"[A-Z]");
        if (!strength.HasUppercase)
        {
            errors.Add("Password must contain at least one uppercase letter");
            strength.Feedback.Add("Add an uppercase letter");
        }

        // Check for lowercase letter
        strength.HasLowercase = Regex.IsMatch(password, @"[a-z]");
        if (!strength.HasLowercase)
        {
            errors.Add("Password must contain at least one lowercase letter");
            strength.Feedback.Add("Add a lowercase letter");
        }

        // Check for number
        strength.HasNumber = Regex.IsMatch(password, @"\d");
        if (!strength.HasNumber)
        {
            errors.Add("Password must contain at least one number");
            strength.Feedback.Add("Add a number");
        }

        // Check for special character
        strength.HasSpecialChar = Regex.IsMatch(password, @"[@$!%*?&]");
        if (!strength.HasSpecialChar)
        {
            errors.Add("Password must contain at least one special character (@$!%*?&)");
            strength.Feedback.Add("Add a special character (@$!%*?&)");
        }

        // Calculate strength score
        strength.Score = CalculateStrengthScore(password, strength);

        // Add feedback based on score
        switch (strength.Score)
        {
            case 0:
            case 1:
                strength.Feedback.Add("Very weak password");
                break;
            case 2:
                strength.Feedback.Add("Weak password");
                break;
            case 3:
                strength.Feedback.Add("Fair password");
                break;
            case 4:
                strength.Feedback.Add("Good password");
                break;
            case 5:
                strength.Feedback.Add("Strong password");
                break;
        }

        result.IsValid = errors.Count == 0;
        result.Errors = errors;
        result.Strength = strength;

        return result;
    }

    public bool IsPasswordStrong(string password)
    {
        var result = ValidatePassword(password);
        return result.IsValid && result.Strength.Score >= 4;
    }

    private int CalculateStrengthScore(string password, PasswordStrength strength)
    {
        int score = 0;

        // Base requirements
        if (strength.HasMinLength) score++;
        if (strength.HasUppercase) score++;
        if (strength.HasLowercase) score++;
        if (strength.HasNumber) score++;
        if (strength.HasSpecialChar) score++;

        // longer passwords
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        // variety of special characters
        if (Regex.Matches(password, @"[@$!%*?&]").Count > 1) score++;

        // cap at 5
        return Math.Min(score, 5);
    }
}
