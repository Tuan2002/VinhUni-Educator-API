using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Helpers;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly ILogger<AuthServices> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtServices _jwtServices;
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheServices _cacheServices;
        private readonly IUserServices _userServices;
        private readonly IMapper _mapper;
        public AuthServices(ILogger<AuthServices> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDBContext context, IJwtServices jwtServices, IHttpContextAccessor httpContextAccessor, ICacheServices cacheServices, IMapper mapper, IUserServices userServices)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtServices = jwtServices;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _cacheServices = cacheServices;
            _mapper = mapper;
            _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _userServices = userServices;
        }
        public async Task<ActionResponse> LoginAsync(LoginModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.UserName) ?? await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Tên người dùng hoặc mật khẩu không chính xác"
                    };
                }
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (!result.Succeeded)
                {
                    return new ActionResponse
                    {
                        StatusCode = 404,
                        IsSuccess = false,
                        Message = "Tên người dùng hoặc mật khẩu không chính xác"
                    };
                }
                var userRoles = await _userManager.GetRolesAsync(user);
                var tokenClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(type: "LastName", user.LastName ?? ""),
                    new Claim(type: "FirstName", user.FirstName ?? ""),
                };
                tokenClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
                var accessTokenResponse = _jwtServices.GenerateAccessToken(tokenClaims);
                var refreshTokenResponse = _jwtServices.GenerateRefreshToken(tokenClaims);
                if (accessTokenResponse == null || refreshTokenResponse == null)
                {
                    throw new InvalidOperationException("Token or Refresh Token invalid.");
                }
                var userRefreshToken = new RefreshToken
                {
                    JwtId = refreshTokenResponse.TokenId,
                    Token = refreshTokenResponse.Token,
                    DateAdded = DateTime.UtcNow,
                    DateExpire = refreshTokenResponse.Expiration,
                    IsUsed = false,
                    IsRevoked = false,
                    UserId = user.Id
                };
                var saveCache = await _cacheServices.SetDataAsync<RefreshToken>(refreshTokenResponse.TokenId, userRefreshToken, new DateTimeOffset(refreshTokenResponse.Expiration));
                _context.RefreshTokens.Add(userRefreshToken);
                _context.SaveChanges();
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", refreshTokenResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = refreshTokenResponse.Expiration
                });
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Đăng nhập thành công",
                    Data = new
                    {
                        accessToken = accessTokenResponse.Token,
                        refreshToken = refreshTokenResponse.Token
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while logging in: {ex.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Lỗi máy chủ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> LoginSSOAsync(LoginModel model)
        {
            try
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
                // Start login process with SSO
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
                List<Claim> claims = _jwtServices.GetTokenClaims(uSmartAccessToken);
                string? smartUserId = claims?.FirstOrDefault(c => c.Type == "userid")?.Value.ToString();
                if (string.IsNullOrEmpty(smartUserId))
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Lỗi xác thực, vui lòng thử lại"
                    };
                }
                var user = _context.Users.Where(user => user.USmartId == int.Parse(smartUserId)).FirstOrDefault();
                if (user == null)
                {
                    var createResponse = await _userServices.SyncUserFromSSO(uSmartAccessToken);
                    if (!createResponse.IsSuccess)
                    {
                        return new ActionResponse
                        {
                            StatusCode = createResponse.StatusCode,
                            IsSuccess = false,
                            Message = createResponse.Message
                        };
                    }
                    user = _context.Users.Where(user => user.USmartId == int.Parse(smartUserId)).FirstOrDefault();
                }
                if (user == null)
                {
                    return new ActionResponse
                    {
                        StatusCode = 500,
                        IsSuccess = false,
                        Message = "Lỗi máy chủ, vui lòng thử lại sau"
                    };
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var tokenClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? ""),
                    new Claim(type: "LastName", user.LastName ?? ""),
                    new Claim(type: "FirstName", user.FirstName ?? ""),
                };
                tokenClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
                var accessTokenResponse = _jwtServices.GenerateAccessToken(tokenClaims);
                var refreshTokenResponse = _jwtServices.GenerateRefreshToken(tokenClaims);
                if (accessTokenResponse == null || refreshTokenResponse == null)
                {
                    throw new InvalidOperationException("Token or Refresh Token invalid.");
                }
                var userRefreshToken = new RefreshToken
                {
                    JwtId = refreshTokenResponse.TokenId,
                    Token = refreshTokenResponse.Token,
                    DateAdded = DateTime.UtcNow,
                    DateExpire = refreshTokenResponse.Expiration,
                    IsUsed = false,
                    IsRevoked = false,
                    UserId = user.Id
                };
                var uSmartToken = _context.USmartTokens.Where(ut => ut.UserId == user.Id).FirstOrDefault();
                if (uSmartToken != null)
                {
                    uSmartToken.Token = uSmartAccessToken;
                    uSmartToken.ExpireDate = _jwtServices.GetTokenExpiration(uSmartAccessToken);
                    uSmartToken.IsExpired = false;
                    _context.USmartTokens.Update(uSmartToken);
                }
                else
                {
                    uSmartToken = new USmartToken
                    {
                        Token = uSmartAccessToken,
                        UserId = user.Id,
                        ExpireDate = _jwtServices.GetTokenExpiration(uSmartAccessToken),
                        IsExpired = false
                    };
                    _context.USmartTokens.Add(uSmartToken);
                }
                var saveCache = await _cacheServices.SetDataAsync<RefreshToken>(refreshTokenResponse.TokenId, userRefreshToken, new DateTimeOffset(refreshTokenResponse.Expiration));
                _context.RefreshTokens.Add(userRefreshToken);
                _context.SaveChanges();
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", refreshTokenResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = refreshTokenResponse.Expiration
                });
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Đăng nhập thành công",
                    Data = new
                    {
                        accessToken = accessTokenResponse.Token,
                        refreshToken = refreshTokenResponse.Token
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while logging in: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Lỗi máy chủ, vui lòng thử lại sau"
                };
            }
        }
        public async Task<ActionResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Check token from user request
                var tokenClaims = _jwtServices.GetTokenClaims(refreshToken);
                var refreshTokenId = tokenClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var userId = tokenClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(refreshTokenId) || userId == null || !_jwtServices.ValidateRefreshToken(refreshToken))
                {
                    throw new InvalidOperationException("Token invalid");
                }
                // Check token from cache or database
                var storedRefreshToken = (await _cacheServices.GetDataAsync<RefreshToken>(refreshTokenId) ?? await _context.RefreshTokens.Where(rt => rt.JwtId == refreshTokenId).FirstOrDefaultAsync()) ?? throw new InvalidOperationException("Token invalid");
                var storedTokenClaims = _jwtServices.GetTokenClaims(storedRefreshToken.Token);
                var isClaimValid = storedTokenClaims.All(c => tokenClaims.Any(tc => tc.Type == c.Type && tc.Value == c.Value));
                if (storedRefreshToken.IsUsed || !isClaimValid || !_jwtServices.ValidateRefreshToken(storedRefreshToken.Token))
                {
                    throw new InvalidOperationException("Token invalid");
                }
                var newRefreshTokenResponse = _jwtServices.GenerateRefreshToken(storedTokenClaims);
                var newAccessTokenResponse = _jwtServices.GenerateAccessToken(storedTokenClaims);
                if (newRefreshTokenResponse == null || newAccessTokenResponse == null)
                {
                    throw new InvalidOperationException("Cannot generate token");
                }
                var userRefreshToken = new RefreshToken
                {
                    JwtId = newRefreshTokenResponse.TokenId,
                    Token = newRefreshTokenResponse.Token,
                    DateAdded = DateTime.UtcNow,
                    DateExpire = newRefreshTokenResponse.Expiration,
                    IsUsed = false,
                    IsRevoked = false,
                    UserId = userId
                };
                var removeCache = await _cacheServices.RemoveDataAsync(refreshTokenId);
                var saveCache = await _cacheServices.SetDataAsync<RefreshToken>(newRefreshTokenResponse.TokenId, userRefreshToken, new DateTimeOffset(newRefreshTokenResponse.Expiration));
                _context.RefreshTokens.Remove(storedRefreshToken);
                _context.RefreshTokens.Add(userRefreshToken);
                _context.SaveChanges();
                _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", newRefreshTokenResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = newRefreshTokenResponse.Expiration
                });
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Token đã được cập nhật",
                    Data = new
                    {
                        accessToken = newAccessTokenResponse.Token,
                        refreshToken = newRefreshTokenResponse.Token
                    }
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while refreshing token: {e.Message} at {DateTime.UtcNow}");
                if (e is InvalidOperationException && e.Message == "Token invalid")
                {
                    return new ActionResponse
                    {
                        StatusCode = 401,
                        IsSuccess = false,
                        Message = "Token không hợp lệ"
                    };
                }
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Error occurred while refreshing token, please try again later or contact administrator"
                };
            }
        }
        public async Task<ActionResponse> LogoutAsync(string accessToken, string refreshToken)
        {
            try
            {
                var accessTokenClaims = _jwtServices.GetTokenClaims(accessToken);
                var refreshTokenClaims = _jwtServices.GetTokenClaims(refreshToken);
                var accessTokenId = accessTokenClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var refreshTokenId = refreshTokenClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(accessTokenId) || string.IsNullOrEmpty(refreshTokenId))
                {
                    throw new InvalidOperationException("Không thể đăng xuất người dùng");
                }
                var addRevokedToken = await _cacheServices.SetDataAsync<string>($"RevokedID:{accessTokenId}", accessToken, _jwtServices.GetRemainingExpiration(accessToken));
                var removeRefreshToken = await _cacheServices.RemoveDataAsync(refreshTokenId);
                _httpContextAccessor?.HttpContext?.Response.Cookies.Delete("refreshToken");
                _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(rt => rt.JwtId == refreshTokenId));
                return new ActionResponse
                {
                    StatusCode = 200,
                    IsSuccess = true,
                    Message = "Đăng xuất thành công"
                };

            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while logging out: {e.Message} at {DateTime.UtcNow}");
                return new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Cannot logout user, please try again later or contact administrator"
                };
            }
        }
    }
}
