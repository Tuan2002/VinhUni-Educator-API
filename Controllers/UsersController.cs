using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    [SwaggerTag("Các chức năng liên quan đến quản lý người dùng")]
    public class UsersController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public UsersController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet]
        [Route("get-users")]
        [SwaggerOperation(Summary = "Lấy danh sách người dùng", Description = "Lấy danh sách người dùng")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] int? pageIndex, [FromQuery] int? limit)
        {
            var response = await _userServices.GetUsersAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-users")]
        [SwaggerOperation(Summary = "Lấy danh sách người dùng đã xóa", Description = "Lấy danh sách người dùng đã xóa")]
        public async Task<IActionResult> GetDeletedUsersAsync([FromQuery] int? pageIndex, [FromQuery] int? limit)
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
    }
}