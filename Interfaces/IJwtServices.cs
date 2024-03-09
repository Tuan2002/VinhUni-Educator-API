using System.Security.Claims;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IJwtServices
    {
        string? GenerateAccessToken(List<Claim> authClaims);
        string? GenerateRefreshToken(List<Claim> authClaims);
        bool ValidateRefreshToken(string token);
        bool ValidateAccessToken(string token);
        bool IsTokenExpired(string token);
        List<Claim> GetTokenClaims(string token);


    }
}