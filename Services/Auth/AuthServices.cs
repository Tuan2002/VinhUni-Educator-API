using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services.Auth
{
    public class AuthServices : IAuthServices
    {
        private readonly ILogger<AuthServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthServices(ILogger<AuthServices> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDBContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ActionResponse> LoginAsync(LoginModel model)
        {
            var response = new ActionResponse();
            var userName = model.UserName ?? string.Empty;
            if (string.IsNullOrEmpty(userName))
            {
                response.IsSuccess = false;
                response.Message = "Email hoặc tên người dùng không được để trống";
                return response;
            }
            try
            {
                var user = await _userManager.FindByEmailAsync(userName) ?? await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    response.StatusCode = 401;
                    response.IsSuccess = false;
                    response.Message = "Tên người dùng hoặc mật khẩu không chính xác";
                    return response;
                }
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (!result.Succeeded)
                {
                    response.StatusCode = 401;
                    response.IsSuccess = false;
                    response.Message = "Tên người dùng hoặc mật khẩu không chính xác";
                    return response;
                }
                var userRoles = await _userManager.GetRolesAsync(user);
                var refreshTokenId = Guid.NewGuid().ToString();
                var accessTokenId = Guid.NewGuid().ToString();
                var accessTokenClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, accessTokenId),
                };
                var refreshTokenClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, refreshTokenId),
                };
                foreach (var userRole in userRoles)
                {
                    accessTokenClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int tokenValidityInDays);
                var helper = new JWTHelper(_configuration, _logger);
                var accessToken = helper.CreateToken(accessTokenClaims);
                var refreshToken = helper.GenerateRefreshToken(refreshTokenClaims);
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    throw new InvalidOperationException("Token or Refresh Token invalid.");
                }
                var userRefreshToken = new RefreshToken
                {
                    JwtId = refreshTokenId,
                    Token = refreshToken,
                    DateAdded = DateTime.UtcNow,
                    DateExpire = DateTime.UtcNow.AddDays(tokenValidityInDays),
                    IsUsed = false,
                    IsRevoked = false,
                    UserId = user.Id
                };
                _context.RefreshTokens.Add(userRefreshToken);
                await _context.SaveChangesAsync();
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(tokenValidityInDays)
                });
                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Đăng nhập thành công";
                response.Data = new
                {
                    accessToken,
                    refreshToken
                };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                response.IsSuccess = false;
                response.Message = "Lỗi máy chủ, vui lòng thử lại sau";
                return response;
            }
        }
        public async Task<ActionResponse> LoginSSOAsync(LoginModel model)
        {
            return new ActionResponse();
        }
    }
}
