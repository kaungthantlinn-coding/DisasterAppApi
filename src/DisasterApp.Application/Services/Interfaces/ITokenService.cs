namespace DisasterApp.Application.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateLoginToken(Guid userId);
        Task<Guid?> ValidateLoginTokenAsync(string loginToken);
    }
}//
