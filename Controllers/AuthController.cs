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
                    return Unauthorized(response);
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
                    Message = "Có lỗi xảy ra, vui lòng thử lại sau"
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
                    return Unauthorized(response);
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
                    Message = "Có lỗi xảy ra, vui lòng thử lại sau"
                });
            }
        }
        [HttpGet]
        [Route("refresh-token")]
        [SwaggerOperation(Summary = "Lấy access-token mới", Description = "Lấy access-token khi token đã hết hạn")]
        public async Task<IActionResult> RefreshToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(
                    new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Token không hợp lệ"
                    }
                );
            }
            try
            {
                var response = await _authServices.RefreshTokenAsync(token);
                if (!response.IsSuccess)
                {
                    return Unauthorized(response);
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
                    Message = "Có lỗi xảy ra, vui lòng thử lại sau"
                });
            }
        }
    }
}