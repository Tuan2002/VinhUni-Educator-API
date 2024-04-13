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
    [SwaggerTag("Quản lý danh sách sinh viên")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentServices _studentServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;

        public StudentsController(IStudentServices studentServices)
        {
            _studentServices = studentServices;
        }

        [HttpGet]
        [Route("get-students")]
        [SwaggerOperation(Summary = "Lấy danh sách sinh viên", Description = "Lấy danh sách sinh viên từ hệ thống")]
        public async Task<IActionResult> GetStudentsAsync(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _studentServices.GetStudentsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-importable/{classId}")]
        [SwaggerOperation(Summary = "Lấy danh sách sinh viên có thể nhập vào hệ thống", Description = "Lấy danh sách sinh viên có thể nhập vào hệ thống theo lớp hành chính")]
        public async Task<IActionResult> GetImportableStudents(int classId)
        {
            var response = await _studentServices.GetAImportableStudentsByClassAsync(classId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("import-students/{classId}")]
        [SwaggerOperation(Summary = "Nhập danh sách sinh viên vào hệ thống", Description = "Nhập danh sách sinh viên vào hệ thống theo lớp hành chính")]
        public async Task<IActionResult> ImportStudents(int classId, [FromBody] List<ImportStudentModel> students)
        {
            var response = await _studentServices.ImportStudentByClass(classId, students);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-student-by-id/{studentId}")]
        [SwaggerOperation(Summary = "Lấy thông tin sinh viên theo ID", Description = "Lấy thông tin sinh viên theo ID")]
        public async Task<IActionResult> GetStudentById(int studentId)
        {
            var response = await _studentServices.GetStudentByIdAsync(studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-student-by-code/{studentCode}")]
        [SwaggerOperation(Summary = "Lấy thông tin sinh viên theo mã sinh viên", Description = "Lấy thông tin sinh viên theo mã sinh viên")]
        public async Task<IActionResult> GetStudentByCode(string studentCode)
        {
            var response = await _studentServices.GetStudentByCodeAsync(studentCode);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-students-by-class/{classId}")]
        [SwaggerOperation(Summary = "Lấy danh sách sinh viên theo lớp hành chính", Description = "Lấy danh sách sinh viên theo lớp hành chính")]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var response = await _studentServices.GetStudentsByClassAsync(classId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-student/{studentId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin sinh viên", Description = "Cập nhật thông tin sinh viên")]
        public async Task<IActionResult> UpdateStudent(int studentId, [FromBody] UpdateStudentModel model)
        {
            var response = await _studentServices.UpdateStudentAsync(studentId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-student/{studentId}")]
        [SwaggerOperation(Summary = "Xóa sinh viên", Description = "Xóa sinh viên")]
        public async Task<IActionResult> DeleteStudent(int studentId)
        {
            var response = await _studentServices.DeleteStudentAsync(studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-student/{studentId}")]
        [SwaggerOperation(Summary = "Khôi phục sinh viên", Description = "Khôi phục sinh viên")]
        public async Task<IActionResult> RestoreStudent(int studentId)
        {
            var response = await _studentServices.RestoreStudentAsync(studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("link-account/{studentId}")]
        [SwaggerOperation(Summary = "Liên kết tài khoản sinh viên", Description = "Liên kết tài khoản người dùng với sinh viên")]
        public async Task<IActionResult> LinkAccount(int studentId, [FromQuery] string userId)
        {
            var response = await _studentServices.LinkUserAccountAsync(studentId, userId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("unlink-account/{studentId}")]
        [SwaggerOperation(Summary = "Hủy liên kết tài khoản sinh viên", Description = "Hủy liên kết tài khoản người dùng với sinh viên")]
        public async Task<IActionResult> UnlinkAccount(int studentId)
        {
            var response = await _studentServices.UnlinkUserAccountAsync(studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search")]
        [SwaggerOperation(Summary = "Tìm kiếm sinh viên", Description = "Tìm kiếm sinh viên theo từ khóa")]
        public async Task<IActionResult> SearchStudent([FromQuery] string? searchKey, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _studentServices.SearchStudentAsync(searchKey, limit);
            return StatusCode(response.StatusCode, response);
        }
    }
}