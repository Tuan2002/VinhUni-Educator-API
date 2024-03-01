using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RestSharp;
using RestSharp.Authenticators;
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
            var SSOBaseURL = _configuration["VinhUNISmart:SSO"];
            var APIBaseURL = _configuration["VinhUNISmart:API"];
            if (string.IsNullOrEmpty(SSOBaseURL) || string.IsNullOrEmpty(APIBaseURL))
            {
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Lỗi máy chủ, vui lòng thử lại sau"
                };
            }
            var cookieContainer = new CookieContainer();
            var SSOClient = new RestClient(new RestClientOptions(SSOBaseURL) { FollowRedirects = false, CookieContainer = cookieContainer });
            SSOClient.AddDefaultHeader("Accept", "*/*");
            SSOClient.AddDefaultHeader("Accept-Encoding", "gzip, deflate, br");
            var loginRequest = new RestRequest("Account/Login");
            var parameters = new
            {
                ReturnUrl = @"/connect/authorize/callback?response_type=id_token%20token&client_id=e-university&state=gUQBhuNvp96V1sPDRgKdmMmJUPykbVWtP1T7dELO&redirect_uri=https%3A%2F%2Fusmart.vinhuni.edu.vn&scope=openid%20profile%20email&nonce=gUQBhuNvp96V1sPDRgKdmMmJUPykbVWtP1T7dELO",
                FromApp = "False",
                model.UserName,
                model.Password,
                button = "login",
                __RequestVerification = "CfDJ8HRX9aXUghNMsXnm0Zl5yNgTEGa8hbYNa7ePwwXIzNiI3hWh2uuv6nQDhW2RZzBcQ7UR1M9UXWJShJdWHlTFkDRL585ECRpkpHq2AvadtGWJjqTNDh79OmkMBHyWvZyCeWJkyI-I3wMzhbh629j3KIg",

            };
            loginRequest.AddObject(parameters);
            var loginResponse = await SSOClient.ExecutePostAsync(loginRequest);
            var cookies = loginResponse.Cookies;
            if ((int)loginResponse.StatusCode != 302)
            {
                return new ActionResponse
                {
                    StatusCode = (int)loginResponse.StatusCode,
                    IsSuccess = false,
                    Message = "Lỗi xác thực, vui lòng thử lại"
                };
            }
            var getTokenRequest = new RestRequest("connect/authorize/callback");
            getTokenRequest.AddQueryParameter("response_type", "id_token token");
            getTokenRequest.AddQueryParameter("client_id", "e-university");
            getTokenRequest.AddQueryParameter("state", "gUQBhuNvp96V1sPDRgKdmMmJUPykbVWtP1T7dELO");
            getTokenRequest.AddQueryParameter("redirect_uri", "https://usmart.vinhuni.edu.vn");
            getTokenRequest.AddQueryParameter("scope", "openid profile email");
            getTokenRequest.AddQueryParameter("nonce", "gUQBhuNvp96V1sPDRgKdmMmJUPykbVWtP1T7dELO");
            var getTokenResponse = await SSOClient.ExecuteGetAsync(getTokenRequest);
            var location = getTokenResponse.Headers?.FirstOrDefault(x => x.Name == "Location")?.Value?.ToString();
            var URLParser = new URLParser();
            var fragments = URLParser.ParseFragments(location);
            if (fragments == null)
            {
                return new ActionResponse
                {
                    StatusCode = (int)getTokenResponse.StatusCode,
                    IsSuccess = false,
                    Message = "Lỗi xác thực, vui lòng thử lại"
                };
            }
            var uSmartAccessToken = fragments.access_token;
            var JWTHelper = new JWTHelper(_configuration, _logger);
            List<Claim> claims = JWTHelper.GetClaims(uSmartAccessToken);
            string? userId = claims?.FirstOrDefault(c => c.Type == "userid")?.Value.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return new ActionResponse
                {
                    StatusCode = (int)getTokenResponse.StatusCode,
                    IsSuccess = false,
                    Message = "Lỗi xác thực, vui lòng thử lại"
                };
            }
            var user = _context.Users.Where(user => user.USmartId == int.Parse(userId)).FirstOrDefault();
            if (user == null)
            {
                return new ActionResponse
                {
                    StatusCode = 404,
                    IsSuccess = false,
                    Message = "Tài khoản không tồn tại"
                };
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
            return new ActionResponse
            {
                StatusCode = 200,
                IsSuccess = true,
                Message = "Đăng nhập thành công",
                Data = new
                {
                    accessToken,
                    refreshToken
                }
            };
        }
    }
}
