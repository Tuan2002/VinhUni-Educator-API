using System.ComponentModel.DataAnnotations;
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
    [Authorize]
    [SwaggerTag("Các chức năng liên quan đến tài khoản người dùng")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountServices _accountServices;
        public AccountController(IAccountServices accountServices)
        {
            _accountServices = accountServices;
        }
        [HttpGet]
        [Route("me")]
        [SwaggerOperation(Summary = "Lấy thông tin người dùng hiện tại", Description = "Lấy thông tin người dùng hiện tại")]
        public async Task<IActionResult> GetCurrentUser(bool? skipCache)
        {
            var response = await _accountServices.GetCurrentUserAsync(skipCache);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("student-profile")]
        [SwaggerOperation(Summary = "Lấy thông tin sinh viên", Description = "Lấy thông tin sinh viên")]
        public async Task<IActionResult> GetStudentProfile()
        {
            var response = await _accountServices.GetStudentProfileAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("teacher-profile")]
        [SwaggerOperation(Summary = "Lấy thông tin giảng viên", Description = "Lấy thông tin giảng viên")]
        public async Task<IActionResult> GetTeacherProfile()
        {
            var response = await _accountServices.GetTeacherProfileAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("set-password")]
        [SwaggerOperation(Summary = "Thay đổi mật khẩu lần đầu", Description = "Thay đổi mật khẩu lần đầu sau khi đăng nhập tài khoản")]
        public async Task<IActionResult> FirstChangePassword([FromBody][Required] ResetPasswordModel model)
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
        [Route("change-password")]
        [SwaggerOperation(Summary = "Thay đổi mật khẩu", Description = "Thay đổi mật khẩu người dùng")]
        public async Task<IActionResult> ChangePassword([FromBody][Required] ChangePasswordModel model)
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
        [HttpPost]
        [Route("forgot-password")]
        [SwaggerOperation(Summary = "Quên mật khẩu", Description = "Gửi mã OTP để đổi mật khẩu")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var response = await _accountServices.ForgotPasswordAsync(email);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("verify-otp")]
        [SwaggerOperation(Summary = "Xác nhận mã OTP", Description = "Xác nhận mã OTP để đổi mật khẩu")]
        public async Task<IActionResult> VerifyOTP([FromBody] string otp)
        {
            var response = await _accountServices.VerifyOTPAsync(otp);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("reset-password")]
        [SwaggerOperation(Summary = "Đặt lại mật khẩu", Description = "Đặt lại mật mật khẩu người dùng")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
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
            var response = await _accountServices.ResetPasswordAsync(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-profile")]
        [SwaggerOperation(Summary = "Cập nhật thông tin cá nhân", Description = "Cập nhật thông tin cá nhân người dùng")]
        public async Task<IActionResult> UpdateProfile([FromBody][Required] UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Thông tin cập nhật không hợp lệ"
                    }
                );
            }
            var response = await _accountServices.UpdateProfileAsync(model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("change-avatar")]
        [SwaggerOperation(Summary = "Thay đổi ảnh đại diện", Description = "Thay đổi ảnh đại diện người dùng")]
        public async Task<IActionResult> UploadProfileImage([FromBody][Required] UploadProfileImage model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ActionResponse
                    {
                        StatusCode = 400,
                        IsSuccess = false,
                        Message = "Thông tin ảnh không hợp lệ"
                    }
                );
            }
            var response = await _accountServices.UploadProfileImageAsync(model);
            return StatusCode(response.StatusCode, response);

        }
    }
}