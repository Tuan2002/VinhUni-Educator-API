using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Các chức năng liên quan đến quản lý người dùng")]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public UsersController(IUserServices userServices)
        {
            _userServices = userServices;
        }
        [HttpPost]
        [Route("create-user")]
        [SwaggerOperation(Summary = "Thêm người dùng mới", Description = "Thêm người dùng vào hệ thống")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _userServices.CreateUserAsync(model);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-users")]
        [SwaggerOperation(Summary = "Lấy danh sách người dùng", Description = "Lấy danh sách người dùng")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _userServices.GetUsersAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-user-byId/{userId}")]
        [SwaggerOperation(Summary = "Lấy thông tin người dùng bằng mã người dùng", Description = "Lấy thông tin người dùng")]
        public async Task<IActionResult> GetUserByIdAsync(string userId)
        {
            var response = await _userServices.GetUserByIdAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-user-byName/{userName}")]
        [SwaggerOperation(Summary = "Lấy thông tin người dùng bằng tên tài khoản", Description = "Lấy thông tin người dùng")]
        public async Task<IActionResult> GetUserByNameAsync(string userName)
        {
            var response = await _userServices.GetUserByNameAsync(userName);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [Route("get-deleted-users")]
        [SwaggerOperation(Summary = "Lấy danh sách người dùng đã xóa", Description = "Lấy danh sách người dùng đã xóa")]
        public async Task<IActionResult> GetDeletedUsersAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _userServices.GetDeletedUsersAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-user/{userId}")]
        [SwaggerOperation(Summary = "Xóa người dùng", Description = "Xóa người dùng")]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            var response = await _userServices.DeleteUserAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-user/{userId}")]
        [SwaggerOperation(Summary = "Khôi phục người dùng", Description = "Khôi phục người dùng")]
        public async Task<IActionResult> RestoreUserAsync(string userId)
        {
            var response = await _userServices.RestoreUserAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-user/{userId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin người dùng", Description = "Cập nhật thông tin người dùng")]
        public async Task<IActionResult> UpdateUserAsync(string userId, [FromBody][Required] UpdateProfileModel model)
        {
            var response = await _userServices.UpdateUserAsync(userId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("reset-password/{userId}")]
        [SwaggerOperation(Summary = "Đặt lại mật khẩu người dùng", Description = "Đặt lại mật khẩu người dùng")]
        public async Task<IActionResult> ResetPasswordAsync(string userId, [FromBody][Required] ResetPasswordModel model)
        {
            var response = await _userServices.ResetUserPasswordAsync(userId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search")]
        [SwaggerOperation(Summary = "Tìm kiếm người dùng", Description = "Tìm kiếm người dùng")]
        public async Task<IActionResult> SearchUsersAsync([FromQuery] string? keyword, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _userServices.SearchUsersAsync(keyword, limit);
            return StatusCode(response.StatusCode, response);
        }
    }
}