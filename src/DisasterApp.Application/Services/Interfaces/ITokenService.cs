namespace DisasterApp.Application.Services.Interfaces;

/// <summary>
/// Service interface for token operations
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate a login token for 2FA verification
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Login token</returns>
    string GenerateLoginToken(Guid userId);

    /// <summary>
    /// Validate a login token and extract user ID
    /// </summary>
    /// <param name="loginToken">Login token to validate</param>
    /// <returns>User ID if valid, null otherwise</returns>
    Task<Guid?> ValidateLoginTokenAsync(string loginToken);
}
