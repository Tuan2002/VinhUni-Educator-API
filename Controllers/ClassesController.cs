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
    [SwaggerTag("Quản lý danh mục lớp hành chính")]
    public class ClassesController : ControllerBase
    {
        private readonly IPrimaryClassServices _primaryClassServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public ClassesController(IPrimaryClassServices primaryClassServices)
        {
            _primaryClassServices = primaryClassServices;
        }

        [HttpPost]
        [Route("sync")]
        [SwaggerOperation(Summary = "Đồng bộ danh sách lớp hành chính", Description = "Đồng bộ danh sách lớp hành chính từ hệ thống Đại học Vinh")]
        public async Task<IActionResult> SyncPrimaryClasses()
        {
            var response = await _primaryClassServices.SyncPrimaryClassesAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-classes")]
        [SwaggerOperation(Summary = "Lấy danh sách lớp hành chính", Description = "Lấy danh sách lớp hành chính từ hệ thống")]
        public async Task<IActionResult> GetPrimaryClasses([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _primaryClassServices.GetPrimaryClassesAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-classes")]
        [SwaggerOperation(Summary = "Lấy danh sách lớp hành chính đã xóa", Description = "Lấy danh sách lớp hành chính đã xóa khỏi hệ thống")]
        public async Task<IActionResult> GetDeletedPrimaryClasses([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _primaryClassServices.GetDeletedPrimaryClassesAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-class/{classId}")]
        [SwaggerOperation(Summary = "Lấy thông tin lớp hành chính", Description = "Lấy thông tin lớp hành chính từ hệ thống")]
        public async Task<IActionResult> GetPrimaryClassById(int classId)
        {
            var response = await _primaryClassServices.GetPrimaryClassByIdAsync(classId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-class/{classId}")]
        [SwaggerOperation(Summary = "Xóa lớp hành chính", Description = "Xóa lớp hành chính khỏi hệ thống")]
        public async Task<IActionResult> DeletePrimaryClass(int classId)
        {
            var response = await _primaryClassServices.DeletePrimaryClassAsync(classId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-class/{classId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin lớp hành chính", Description = "Cập nhật thông tin lớp hành chính trong hệ thống")]
        public async Task<IActionResult> UpdatePrimaryClass(int classId, [FromBody] UpdateClassModel model)
        {
            var response = await _primaryClassServices.UpdatePrimaryClassAsync(classId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-classes-by-course/{courseId}")]
        [SwaggerOperation(Summary = "Lấy danh sách lớp hành chính theo khóa học", Description = "Lấy danh sách lớp hành chính theo khóa học từ hệ thống")]
        public async Task<IActionResult> GetPrimaryClassesByCourse(int courseId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _primaryClassServices.GetPrimaryClassesByCourseAsync(courseId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-classes-by-program/{programId}")]
        [SwaggerOperation(Summary = "Lấy danh sách lớp hành chính theo chương trình đào tạo", Description = "Lấy danh sách lớp hành chính theo chương trình đào tạo từ hệ thống")]
        public async Task<IActionResult> GetPrimaryClassesByProgram(int programId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _primaryClassServices.GetPrimaryClassesByProgramAsync(programId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search")]
        [SwaggerOperation(Summary = "Tìm kiếm lớp hành chính", Description = "Tìm kiếm lớp hành chính theo từ khóa")]
        public async Task<IActionResult> SearchPrimaryClasses([FromQuery] string? keyword, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _primaryClassServices.SearchPrimaryClassesAsync(keyword, limit);
            return StatusCode(response.StatusCode, response);
        }
    }
}