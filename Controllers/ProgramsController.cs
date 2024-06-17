using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Configs;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý chương trình đào tạo")]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramServices _programServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public ProgramsController(IProgramServices programServices)
        {
            _programServices = programServices;
        }
        [HttpPost]
        [Route("sync")]
        [SwaggerOperation(Summary = "Đồng bộ chương trình đào tạo từ hệ thống USmart")]
        public async Task<IActionResult> SyncProgramsAsync()
        {
            var response = await _programServices.SyncProgramsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-programs")]
        [SwaggerOperation(Summary = "Lấy danh sách chương trình đào tạo", Description = "Lấy danh sách chương trình đào tạo từ hệ thống")]
        public async Task<IActionResult> GetProgramsAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _programServices.GetProgramsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-programs")]
        [SwaggerOperation(Summary = "Lấy danh sách chương trình đào tạo đã xóa", Description = "Lấy danh sách chương trình đào tạo đã xóa khỏi hệ thống")]
        public async Task<IActionResult> GetDeletedProgramsAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _programServices.GetDeletedProgramsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-program/{programId}")]
        [SwaggerOperation(Summary = "Lấy thông tin chương trình đào tạo", Description = "Lấy thông tin chương trình đào tạo từ hệ thống")]
        public async Task<IActionResult> GetProgramByIdAsync(int programId)
        {
            var response = await _programServices.GetProgramByIdAsync(programId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-program/{programId}")]
        [SwaggerOperation(Summary = "Xóa chương trình đào tạo", Description = "Xóa chương trình đào tạo khỏi hệ thống")]
        public async Task<IActionResult> DeleteProgramAsync(int programId)
        {
            var response = await _programServices.DeleteProgramAsync(programId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-program/{programId}")]
        [SwaggerOperation(Summary = "Khôi phục chương trình đào tạo", Description = "Khôi phục chương trình đào tạo đã bị xóa khỏi hệ thống")]
        public async Task<IActionResult> RestoreProgramAsync(int programId)
        {
            var response = await _programServices.RestoreProgramAsync(programId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-program/{programId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin chương trình đào tạo", Description = "Cập nhật thông tin chương trình đào tạo trong hệ thống")]
        public async Task<IActionResult> UpdateProgramAsync(int programId, [FromBody] UpdateProgramModel model)
        {
            var response = await _programServices.UpdateProgramAsync(programId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-programs-by-major/{majorId}")]
        [SwaggerOperation(Summary = "Lấy danh sách chương trình đào tạo theo ngành học", Description = "Lấy danh sách chương trình đào tạo theo ngành học từ hệ thống")]
        public async Task<IActionResult> GetProgramsByMajorAsync(int majorId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _programServices.GetProgramsByMajorAsync(majorId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-programs-by-course/{courseId}")]
        [SwaggerOperation(Summary = "Lấy danh sách chương trình đào tạo theo khóa học", Description = "Lấy danh sách chương trình đào tạo theo khóa học từ hệ thống")]
        [SwaggerResponse(200, "Danh sách chương trình đào tạo được tìm thấy", typeof(ActionResponse), Description = "Danh sách chương trình đào tạo được tìm thấy trong hệ thống", ContentTypes = ["application/json"])]
        [SwaggerResponse(404, "Không tìm thấy chương trình đào tạo", typeof(ActionResponse), Description = "Không tìm thấy chương trình đào tạo trong hệ thống", ContentTypes = ["application/json"])]
        [SwaggerResponse(500, "Lỗi máy chủ", typeof(ActionResponse), Description = "Lỗi  xảy ở máy chủ", ContentTypes = ["application/json"])]
        public async Task<IActionResult> GetProgramsByCourseAsync(int courseId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _programServices.GetProgramsByCourseAsync(courseId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search")]
        [SwaggerOperation(Summary = "Tìm kiếm chương trình đào tạo", Description = "Tìm kiếm chương trình đào tạo theo từ khóa")]
        [SwaggerResponse(200, "Danh sách chương trình đào tạo được tìm thấy", typeof(ActionResponse), Description = "Danh sách chương trình đào tạo được tìm thấy trong hệ thống", ContentTypes = ["application/json"])]
        [SwaggerResponse(404, "Không tìm thấy chương trình đào tạo", typeof(ActionResponse), Description = "Không tìm thấy chương trình đào tạo trong hệ thống", ContentTypes = ["application/json"])]
        [SwaggerResponse(500, "Lỗi máy chủ", typeof(ActionResponse), Description = "Lỗi  xảy ở máy chủ", ContentTypes = ["application/json"])]
        public async Task<IActionResult> SearchProgramsAsync([FromQuery] string? keyword, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _programServices.SearchProgramsAsync(keyword, limit);
            return StatusCode(response.StatusCode, response);
        }
    }
}