namespace DisasterApp.Application.Services.Interfaces;

public interface IAuditDataSanitizer
{
    string SanitizeForRole(string data, string userRole);
    bool ContainsPII(string data);
    string HashSensitiveData(string data);
    string MaskSensitiveDetails(string details, string userRole);
    Dictionary<string, object>? SanitizeMetadata(Dictionary<string, object>? metadata, string userRole);
    bool HasSensitiveDataAccess(string userRole);
    string RedactEmailAddresses(string text);
    string RedactPhoneNumbers(string text);
    string RedactIpAddresses(string text, string userRole);
}
