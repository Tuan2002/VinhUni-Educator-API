using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý danh sách giáo viên")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherServices _teacherServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public TeachersController(ITeacherServices teacherServices)
        {
            _teacherServices = teacherServices;
        }
        [HttpGet]
        [Route("get-teachers")]
        [SwaggerOperation(Summary = "Lấy danh sách giáo viên", Description = "Lấy danh sách giáo viên từ hệ thống")]
        public async Task<IActionResult> GetTeachersAsync(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _teacherServices.GetTeachersAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-teachers")]
        [SwaggerOperation(Summary = "Lấy danh sách giáo viên đã bị xóa", Description = "Lấy danh sách giáo viên đã bị xóa từ hệ thống")]
        public async Task<IActionResult> GetDeletedTeachersAsync(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _teacherServices.GetDeletedTeachersAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-teachers-by-organization/{organizationId}")]
        [SwaggerOperation(Summary = "Lấy danh sách giáo viên theo phòng ban", Description = "Lấy danh sách giáo viên theo tổ chức")]
        public async Task<IActionResult> GetTeachersByOrganizationAsync(int organizationId)
        {
            var response = await _teacherServices.GetTeachersByOrganizationAsync(organizationId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-teacher-by-id")]
        [SwaggerOperation(Summary = "Lấy thông tin giáo viên theo ID", Description = "Lấy thông tin giáo viên theo ID")]
        public async Task<IActionResult> GetTeacherByIdAsync(int teacherId)
        {
            var response = await _teacherServices.GetTeacherByIdAsync(teacherId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-teacher-by-code")]
        [SwaggerOperation(Summary = "Lấy thông tin giáo viên theo mã giáo viên", Description = "Lấy thông tin giáo viên theo mã giáo viên")]
        public async Task<IActionResult> GetTeacherByCodeAsync(int teacherCode)
        {
            var response = await _teacherServices.GetTeacherByCodeAsync(teacherCode);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-importable/{organizationId}")]
        [SwaggerOperation(Summary = "Lấy danh sách giáo viên có thể nhập vào hệ thống", Description = "Lấy danh sách giáo viên có thể nhập vào hệ thống theo tổ chức")]
        public async Task<IActionResult> GetImportableTeachersByOrganization(int organizationId)
        {
            var response = await _teacherServices.GetImportableTeachersByOrganization(organizationId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("import-teachers/{organizationId}")]
        [SwaggerOperation(Summary = "Nhập danh sách giáo viên vào hệ thống", Description = "Nhập danh sách giáo viên vào hệ thống theo tổ chức")]
        public async Task<IActionResult> ImportTeachersByOrganizationAsync(int organizationId, [FromBody][Required] List<ImportTeacherModel> teachers)
        {
            var response = await _teacherServices.ImportTeachersByOrganizationAsync(organizationId, teachers);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-teacher/{teacherId}")]
        [SwaggerOperation(Summary = "Xóa giáo viên", Description = "Xóa giáo viên khỏi hệ thống")]
        public async Task<IActionResult> DeleteTeacherAsync(int teacherId)
        {
            var response = await _teacherServices.DeleteTeacherAsync(teacherId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-teacher/{teacherId}")]
        [SwaggerOperation(Summary = "Khôi phục giáo viên", Description = "Khôi phục giáo viên đã bị xóa khỏi hệ thống")]
        public async Task<IActionResult> RestoreTeacherAsync(int teacherId)
        {
            var response = await _teacherServices.RestoreTeacherAsync(teacherId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-teacher/{teacherId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin giáo viên", Description = "Cập nhật thông tin giáo viên")]
        public async Task<IActionResult> UpdateTeacherAsync(int teacherId, [FromBody][Required] UpdateTeacherModel model)
        {
            var response = await _teacherServices.UpdateTeacherAsync(teacherId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search-teacher")]
        [SwaggerOperation(Summary = "Tìm kiếm giáo viên", Description = "Tìm kiếm giáo viên theo tên hoặc mã giáo viên")]
        public async Task<IActionResult> SearchTeacherAsync(string? searchKey, int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _teacherServices.SearchTeacherAsync(searchKey, limit);
            return StatusCode(response.StatusCode, response);
        }

    }
}