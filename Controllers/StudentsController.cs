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
        [Route("get-students-by-class/{classId}")]
        [SwaggerOperation(Summary = "Lấy danh sách sinh viên theo lớp hành chính", Description = "Lấy danh sách sinh viên theo lớp hành chính")]
        public async Task<IActionResult> GetStudentsByClass(int classId)
        {
            var response = await _studentServices.GetStudentByClassAsync(classId);
            return StatusCode(response.StatusCode, response);
        }
    }
}