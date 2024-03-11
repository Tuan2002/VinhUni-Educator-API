using System.Security.Claims;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Interfaces
{
    public interface IJwtServices
    {
        TokenResponse? GenerateAccessToken(List<Claim> authClaims);
        TokenResponse? GenerateRefreshToken(List<Claim> authClaims);
        bool ValidateRefreshToken(string token);
        bool ValidateAccessToken(string token);
        bool IsTokenExpired(string? token);
        DateTime? GetTokenExpiration(string token);
        DateTimeOffset GetRemainingExpiration(string token);
        List<Claim> GetTokenClaims(string token);


    }
}