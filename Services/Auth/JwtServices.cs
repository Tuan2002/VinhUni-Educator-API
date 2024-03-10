
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Utils;

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
        public TokenResponse? GenerateAccessToken(List<Claim> authClaims)
        {
            var accessTokenSecret = _configuration["JWT:AccessTokenSecret"];
            if (string.IsNullOrEmpty(accessTokenSecret))
            {
                throw new InvalidOperationException("Access token secret not found");
            }
            try
            {
                var idToken = Guid.NewGuid().ToString();
                var isIdTokenExist = authClaims.Exists(c => c.Type == JwtRegisteredClaimNames.Jti);
                if (isIdTokenExist)
                {
                    authClaims.Remove(authClaims.First(c => c.Type == JwtRegisteredClaimNames.Jti));
                }
                authClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, idToken));
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessTokenSecret));
                _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
                var expiration = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes);
                var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: expiration,
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );
                return new TokenResponse
                {
                    TokenId = idToken,
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    Expiration = expiration
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while creating token: {e.Message} at {DateTime.UtcNow}");
                return default;
            }
        }
        public TokenResponse? GenerateRefreshToken(List<Claim> authClaims)
        {
            var refreshTokenSecret = _configuration["JWT:RefreshTokenSecret"];
            if (string.IsNullOrEmpty(refreshTokenSecret))
            {
                throw new InvalidOperationException("Refresh token secret invalid.");
            }
            try
            {
                var idToken = Guid.NewGuid().ToString();
                var isIdTokenExist = authClaims.Exists(c => c.Type == JwtRegisteredClaimNames.Jti);
                if (isIdTokenExist)
                {
                    authClaims.Remove(authClaims.First(c => c.Type == JwtRegisteredClaimNames.Jti));
                }
                authClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, idToken));
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshTokenSecret));
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int tokenValidityInDays);
                var expiration = DateTime.UtcNow.AddDays(tokenValidityInDays);
                var refreshToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    claims: authClaims,
                    expires: expiration,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );
                return new TokenResponse
                {
                    TokenId = idToken,
                    Token = new JwtSecurityTokenHandler().WriteToken(refreshToken),
                    Expiration = expiration
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while creating refresh token: {e.Message} at {DateTime.UtcNow}");
                return default;
            }
        }
        public List<Claim> GetTokenClaims(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (tokenHandler.ReadToken(token) is JwtSecurityToken securityToken)
                return securityToken.Claims.ToList();
            return new List<Claim>();
        }
        public bool ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessTokenSecret = _configuration["JWT:AccessTokenSecret"];
            var issuer = _configuration["JWT:ValidIssuer"];
            var audience = _configuration["JWT:ValidAudience"];
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
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true; // Token is valid
            }
            catch (SecurityTokenExpiredException)
            {
                return false; // Token is expired
            }
            catch (Exception)
            {
                return false; // Token is invalid
            }
        }
        public bool ValidateRefreshToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var refreshTokenSecret = _configuration["JWT:RefreshTokenSecret"];
            var issuer = _configuration["JWT:ValidIssuer"];
            var audience = _configuration["JWT:ValidAudience"];
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
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true; // Token is valid and not expired
            }
            catch (SecurityTokenExpiredException)
            {
                return false; // Token is expired
            }
            catch (Exception)
            {
                return false; // Token is invalid
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