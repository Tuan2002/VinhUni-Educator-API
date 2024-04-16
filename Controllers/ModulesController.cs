using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý danh mục học phần")]
    public class ModulesController : ControllerBase
    {
        private readonly IModuleServices _moduleServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public ModulesController(IModuleServices moduleServices)
        {
            _moduleServices = moduleServices;
        }
        [HttpPost("sync")]
        [SwaggerOperation("Đồng bộ danh mục học phần từ hệ thống USmart")]
        public async Task<IActionResult> SyncModulesAsync()
        {
            var result = await _moduleServices.SyncModulesAsync();
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("get-modules")]
        [SwaggerOperation("Lấy danh sách học phần từ hệ thống")]
        public async Task<IActionResult> GetModulesAsync(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var result = await _moduleServices.GetModulesAsync(pageIndex, limit);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("get-deleted-modules")]
        [SwaggerOperation("Lấy danh sách học phần đã xóa từ hệ thống")]
        public async Task<IActionResult> GetDeletedModulesAsync(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var result = await _moduleServices.GetDeletedModulesAsync(pageIndex, limit);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("get-module-by-id/{moduleId}")]
        [SwaggerOperation("Lấy thông tin học phần theo Id")]
        public async Task<IActionResult> GetModuleByIdAsync(int moduleId)
        {
            var result = await _moduleServices.GetModuleByIdAsync(moduleId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("get-module-by-code/{moduleCode}")]
        [SwaggerOperation("Lấy thông tin học phần theo mã học phần")]
        public async Task<IActionResult> GetModuleByCodeAsync(string moduleCode)
        {
            var result = await _moduleServices.GetModuleByCodeAsync(moduleCode);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("delete-module/{moduleId}")]
        [SwaggerOperation("Xóa học phần")]
        public async Task<IActionResult> DeleteModuleAsync(int moduleId)
        {
            var result = await _moduleServices.DeleteModuleAsync(moduleId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("restore-module/{moduleId}")]
        [SwaggerOperation("Khôi phục học phần")]
        public async Task<IActionResult> RestoreModuleAsync(int moduleId)
        {
            var result = await _moduleServices.RestoreModuleAsync(moduleId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("search-modules")]
        [SwaggerOperation("Tìm kiếm học phần")]
        public async Task<IActionResult> SearchModulesAsync(string keyword, int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var result = await _moduleServices.SearchModulesAsync(keyword, limit);
            return StatusCode(result.StatusCode, result);
        }
    }
}