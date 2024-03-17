using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Xác thực người dùng")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthServices authServices, ILogger<AuthController> logger)
        {
            _authServices = authServices;
            _logger = logger;
        }
        [HttpPost]
        [Route("login")]
        [SwaggerOperation(Summary = "Đăng nhập bằng tài khoản hệ thống", Description = "Đăng nhập hệ thống bằng tài khoản và mật khẩu")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Thông tin đăng nhập không hợp lệ"
                    }
                );
            }
            try
            {
                var response = await _authServices.LoginAsync(model);
                if (!response.IsSuccess)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while logging in: {e.Message} at {DateTime.UtcNow}");
                return StatusCode(500, new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Error occurred while logging in, please try again later or contact administrator"
                });
            }
        }
        [HttpPost]
        [Route("login-sso")]
        [SwaggerOperation(Summary = "Đăng nhập bằng tài khoản cổng sinh viên", Description = "Đăng nhập hệ thống bằng tài khoản hệ thống trường đại học Đại học Vinh")]
        public async Task<IActionResult> LoginSSO([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Thông tin đăng nhập không hợp lệ"
                    }
                );
            }
            try
            {
                var response = await _authServices.LoginSSOAsync(model);
                if (!response.IsSuccess)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while logging in: {e.Message} at {DateTime.UtcNow}");
                return StatusCode(500, new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Error occurred while logging in, please try again later or contact administrator"
                });
            }
        }
        [HttpGet]
        [Route("me")]
        [Authorize]
        [SwaggerOperation(Summary = "Lấy thông tin người dùng", Description = "Lấy thông tin người dùng hiện tại")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var response = await _authServices.GetCurrentUserAsync();
                if (!response.IsSuccess)
                {
                    throw new Exception(response.Message);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while getting current user: {e.Message} at {DateTime.UtcNow}");
                return StatusCode(500, new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Error occurred while getting current user, please try again later or contact administrator"
                });
            }
        }
        [HttpPost]
        [Route("refresh-token")]
        [SwaggerOperation(Summary = "Lấy access-token mới", Description = "Lấy access-token khi token đã hết hạn")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(
                    new ActionResponse
                    {
                        StatusCode = 401,
                        IsSuccess = false,
                        Message = "You are not authorized, please login to get access"
                    }
                );
            }
            try
            {
                var response = await _authServices.RefreshTokenAsync(refreshToken);
                if (!response.IsSuccess)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);

            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while refreshing token: {e.Message} at {DateTime.UtcNow}");
                return StatusCode(500, new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Error occurred while refreshing token, please try again later or contact administrator"
                });
            }
        }
        [HttpPost]
        [Route("logout")]
        [Authorize]
        [SwaggerOperation(Summary = "Đăng xuất", Description = "Đăng xuất khỏi hệ thống")]
        public async Task<IActionResult> Logout()
        {
            var accessToken = await Request.HttpContext.GetTokenAsync("access_token");
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(
                    new ActionResponse
                    {
                        StatusCode = 401,
                        IsSuccess = false,
                        Message = "You are not authorized, please login to get access"
                    }
                );
            }
            try
            {
                var response = await _authServices.LogoutAsync(accessToken, refreshToken);
                if (!response.IsSuccess)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occurred while logging out: {e.Message} at {DateTime.UtcNow}");
                return StatusCode(500, new ActionResponse
                {
                    StatusCode = 500,
                    IsSuccess = false,
                    Message = "Error occurred while logging out, please try again later or contact administrator"
                });
            }
        }
    }
}