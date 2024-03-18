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
    [SwaggerTag("Các chức năng liên quan đến tài khoản người dùng")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountServices _accountServices;
        public AccountController(IAccountServices accountServices)
        {
            _accountServices = accountServices;
        }
        [HttpGet]
        [Authorize]
        [Route("me")]
        [SwaggerOperation(Summary = "Lấy thông tin người dùng hiện tại", Description = "Lấy thông tin người dùng hiện tại")]
        public async Task<IActionResult> GetCurrentUser(bool? skipCache)
        {
            var response = await _accountServices.GetCurrentUserAsync(skipCache);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Authorize]
        [Route("set-password")]
        [SwaggerOperation(Summary = "Thay đổi mật khẩu lần đầu", Description = "Thay đổi mật khẩu lần đầu sau khi đăng nhập tài khoản")]
        public async Task<IActionResult> FirstChangePassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Thông tin mật khẩu không hợp lệ"
                    }
                );
            }
            var response = await _accountServices.FirstChangePasswordAsync(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Authorize]
        [Route("change-password")]
        [SwaggerOperation(Summary = "Thay đổi mật khẩu", Description = "Thay đổi mật khẩu người dùng")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Thông tin mật khẩu không hợp lệ"
                    }
                );
            }
            var response = await _accountServices.ChangePasswordAsync(model);
            return StatusCode(response.StatusCode, response);
        }

    }
}