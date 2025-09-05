using DisasterApp.Application.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;//
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DisasterApp.Application.Services.Implementations;

public class AuditDataSanitizer : IAuditDataSanitizer
{
    private readonly HashSet<string> _sensitiveDataRoles;
    private readonly Dictionary<string, string> _piiPatterns;
    private readonly string _hashSalt;

    public AuditDataSanitizer()
    {
        _sensitiveDataRoles = new HashSet<string> { "SuperAdmin", "Admin" };
        _piiPatterns = InitializePiiPatterns();
        _hashSalt = "DisasterApp_Audit_Salt_2024"; // In production, use configuration
    }

    public string SanitizeForRole(string data, string userRole)
    {
        if (string.IsNullOrWhiteSpace(data))
            return data;

        if (HasSensitiveDataAccess(userRole))
            return data;      // No sanitization for SuperAdmin and Admin roles

        var sanitized = data;
        sanitized = RedactEmailAddresses(sanitized);
        sanitized = RedactPhoneNumbers(sanitized);
        sanitized = RedactIpAddresses(sanitized, userRole);
        sanitized = MaskSensitiveKeywords(sanitized);

        return sanitized;
    }

    public bool ContainsPII(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return false;

        foreach (var pattern in _piiPatterns.Values)
        {
            if (Regex.IsMatch(data, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }

    public string HashSensitiveData(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return data;

        using var sha256 = SHA256.Create();
        var saltedData = data + _hashSalt;
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedData));
        return Convert.ToBase64String(hashBytes)[..16]; // Take first 16 characters for readability
    }

    public string MaskSensitiveDetails(string details, string userRole)
    {
        if (string.IsNullOrWhiteSpace(details))
            return details;

        if (HasSensitiveDataAccess(userRole))
            return details;

        var masked = details;

        masked = Regex.Replace(masked, @"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", "****-****-****-****", RegexOptions.IgnoreCase);
        masked = Regex.Replace(masked, @"\b\d{3}-\d{2}-\d{4}\b", "***-**-****", RegexOptions.IgnoreCase);
        masked = Regex.Replace(masked, @"password[:\s]*[^\s,]+", "password: [REDACTED]", RegexOptions.IgnoreCase);
        masked = Regex.Replace(masked, @"token[:\s]*[^\s,]+", "token: [REDACTED]", RegexOptions.IgnoreCase);
        masked = Regex.Replace(masked, @"key[:\s]*[^\s,]+", "key: [REDACTED]", RegexOptions.IgnoreCase);

        return masked;
    }

    public Dictionary<string, object>? SanitizeMetadata(Dictionary<string, object>? metadata, string userRole)
    {
        if (metadata == null || !metadata.Any())
            return metadata;

        if (HasSensitiveDataAccess(userRole))
            return metadata;

        var sanitized = new Dictionary<string, object>();

        foreach (var kvp in metadata)
        {
            var key = kvp.Key.ToLowerInvariant();
            var value = kvp.Value?.ToString() ?? "";

            if (IsSensitiveMetadataKey(key))
            {
                sanitized[kvp.Key] = "[REDACTED]";
            }
            else if (ContainsPII(value))
            {
                sanitized[kvp.Key] = SanitizeForRole(value, userRole);
            }
            else
            {
                sanitized[kvp.Key] = kvp.Value;
            }
        }

        return sanitized;
    }

    public bool HasSensitiveDataAccess(string userRole)
    {
        return !string.IsNullOrWhiteSpace(userRole) && _sensitiveDataRoles.Contains(userRole);
    }

    public string RedactEmailAddresses(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return Regex.Replace(text, _piiPatterns["email"], match =>
        {
            var email = match.Value;
            var atIndex = email.IndexOf('@');
            if (atIndex > 0)
            {
                var username = email.Substring(0, atIndex);
                var domain = email.Substring(atIndex);
                var maskedUsername = username.Length > 2 
                    ? username.Substring(0, 2) + "***" 
                    : "***";
                return maskedUsername + domain;
            }
            return "***@***.***";
        }, RegexOptions.IgnoreCase);
    }

    public string RedactPhoneNumbers(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return Regex.Replace(text, _piiPatterns["phone"], "***-***-****", RegexOptions.IgnoreCase);
    }

    public string RedactIpAddresses(string text, string userRole)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        if (HasSensitiveDataAccess(userRole))
            return text;

        return Regex.Replace(text, _piiPatterns["ip"], match =>
        {
            var ip = match.Value;
            var parts = ip.Split('.');
            if (parts.Length == 4)
            {
                return $"{parts[0]}.{parts[1]}.***.**";
            }
            return "***.***.***.**";
        }, RegexOptions.IgnoreCase);
    }

    private string MaskSensitiveKeywords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var sensitiveKeywords = new[]
        {
            "password", "token", "secret", "key", "credential", "auth",
            "ssn", "social", "license", "passport", "credit", "debit"
        };

        var masked = text;
        foreach (var keyword in sensitiveKeywords)
        {
            var pattern = $@"\b{keyword}[:\s]*[^\s,]+";
            masked = Regex.Replace(masked, pattern, $"{keyword}: [REDACTED]", RegexOptions.IgnoreCase);
        }

        return masked;
    }

    private bool IsSensitiveMetadataKey(string key)
    {
        var sensitiveKeys = new[]
        {
            "password", "token", "secret", "key", "credential", "auth",
            "ssn", "social", "license", "passport", "credit", "debit",
            "api_key", "access_token", "refresh_token", "session_id"
        };

        return sensitiveKeys.Any(sk => key.Contains(sk, StringComparison.OrdinalIgnoreCase));
    }

    private Dictionary<string, string> InitializePiiPatterns()
    {
        return new Dictionary<string, string>
        {
            ["email"] = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
            ["phone"] = @"\b(?:\+?1[-.\s]?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})\b",
            ["ssn"] = @"\b\d{3}-\d{2}-\d{4}\b",
            ["credit_card"] = @"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b",
            ["ip"] = @"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b",
            ["zip"] = @"\b\d{5}(?:-\d{4})?\b",
            ["date_of_birth"] = @"\b(?:0[1-9]|1[0-2])[\/\-](?:0[1-9]|[12]\d|3[01])[\/\-](?:19|20)\d{2}\b"
        };
    }
}
