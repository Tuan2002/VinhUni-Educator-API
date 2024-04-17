using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/class-modules")]
    [SwaggerTag("Quản lý danh mục lớp học phần")]
    public class ClassModulesController : ControllerBase
    {
        private readonly IClassModuleServices _classModuleServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public ClassModulesController(IClassModuleServices classModuleServices)
        {
            _classModuleServices = classModuleServices;
        }
        [HttpPost("sync-by-admin")]
        [SwaggerOperation("Đồng bộ danh mục lớp học phần theo giảng viên")]
        public async Task<IActionResult> SyncModulesByTeacherIdAsync([FromQuery] int teacherId, [FromQuery] int semesterId)
        {
            var result = await _classModuleServices.SyncModulesByTeacherIdAsync(teacherId, semesterId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("get-by-teacher/{teacherId}")]
        [SwaggerOperation("Lấy danh sách lớp học phần theo giảng viên")]
        public async Task<IActionResult> GetClassByTeacherAsync(int teacherId, [FromQuery] int semesterId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? pageSize = DEFAULT_LIMIT)
        {
            var result = await _classModuleServices.GetClassByTeacherAsync(teacherId, semesterId, pageIndex, pageSize);
            return StatusCode(result.StatusCode, result);
        }


    }
}