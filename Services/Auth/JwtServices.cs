
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Services
{
    public class JwtServices : IJwtServices
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtServices> _logger;
        public JwtServices(IConfiguration configuration, ILogger<JwtServices> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public string? GenerateAccessToken(List<Claim> authClaims)
        {
            var accessTokenSecret = _configuration["JWT:AccessTokenSecret"];
            if (string.IsNullOrEmpty(accessTokenSecret))
            {
                throw new InvalidOperationException("Access token secret invalid.");
            }
            try
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSecret));
                _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while creating token: {e.Message} at {DateTime.UtcNow}");
                return null;
            }
        }
        public string? GenerateRefreshToken(List<Claim> authClaims)
        {
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int tokenValidityInDays);
            var refreshTokenSecret = _configuration["JWT:RefreshTokenSecret"];
            if (string.IsNullOrEmpty(refreshTokenSecret))
            {
                throw new InvalidOperationException("Refresh token secret invalid.");
            }
            try
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshTokenSecret));
                var refreshToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    claims: authClaims,
                    expires: DateTime.Now.AddDays(tokenValidityInDays),
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );
                return new JwtSecurityTokenHandler().WriteToken(refreshToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while creating refresh token: {e.Message} at {DateTime.UtcNow}");
                return null;
            }
        }
        public List<Claim> GetTokenClaims(string token)
        {
            throw new NotImplementedException();
        }
        public bool ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessTokenSecret = _configuration["JWT:AccessTokenSecret"];
            if (string.IsNullOrEmpty(accessTokenSecret))
            {
                throw new InvalidOperationException("Access token secret invalid.");
            }
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return false; // Token is valid
            }
            catch (SecurityTokenExpiredException)
            {
                return true; // Token is expired
            }
            catch (Exception)
            {
                return true; // Token is invalid
            }
        }
        public bool ValidateRefreshToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var refreshTokenSecret = _configuration["JWT:RefreshTokenSecret"];
            if (string.IsNullOrEmpty(refreshTokenSecret))
            {
                throw new InvalidOperationException("Refresh token secret invalid.");
            }
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshTokenSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return false; // Token is valid and not expired
            }
            catch (SecurityTokenExpiredException)
            {
                return true; // Token is expired
            }
            catch (Exception)
            {
                return true; // Token is invalid
            }
        }
        public bool IsTokenExpired(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var checkToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                if (checkToken != null && checkToken.ValidTo < DateTime.UtcNow)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

    }
}