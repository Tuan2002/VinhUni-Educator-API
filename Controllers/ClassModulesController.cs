using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Configs;
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
        private readonly IClassManagerServices _classManagerServices;
        private readonly IExamSeasonServices _examSeasonServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public ClassModulesController(IClassModuleServices classModuleServices, IClassManagerServices classManagerServices, IExamSeasonServices examSeasonServices)
        {
            _classModuleServices = classModuleServices;
            _classManagerServices = classManagerServices;
            _examSeasonServices = examSeasonServices;
        }
        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        [Route("sync-by-admin")]
        [SwaggerOperation("Đồng bộ danh mục lớp học phần theo giảng viên")]
        public async Task<IActionResult> SyncModulesByTeacherIdAsync([FromQuery] int teacherId, [FromQuery] int semesterId)
        {
            var response = await _classModuleServices.SyncClassModulesByTeacherIdAsync(teacherId, semesterId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Authorize(Roles = AppRoles.Teacher)]
        [Route("sync-by-teacher")]
        [SwaggerOperation("Đồng bộ danh mục lớp học phần của giảng viên")]
        public async Task<IActionResult> SyncModulesByTeacherAsync([FromQuery] int semesterId)
        {
            var response = await _classModuleServices.SyncClassModulesByTeacher(semesterId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Teacher}")]
        [Route("get-by-teacher/{teacherId}")]
        [SwaggerOperation("Lấy danh sách lớp học phần theo giảng viên")]
        public async Task<IActionResult> GetClassByTeacherAsync(int teacherId, [FromQuery] int semesterId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? pageSize = DEFAULT_LIMIT, [FromQuery] bool? cached = false)
        {
            var response = await _classModuleServices.GetClassByTeacherAsync(teacherId, semesterId, pageIndex, pageSize, cached);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-by-student/{studentId}")]
        [SwaggerOperation("Lấy danh sách lớp học phần theo sinh viên")]
        public async Task<IActionResult> GetClassByStudentAsync(int studentId, [FromQuery] int semesterId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT, bool? cached = false)
        {
            var response = await _classModuleServices.GetClassByStudentAsync(studentId, semesterId, pageIndex, limit, cached);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-class-modules")]
        [SwaggerOperation("Lấy danh sách lớp học phần")]
        public async Task<IActionResult> GetClassModulesAsync([FromQuery] int semesterId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _classModuleServices.GetClassModulesAsync(semesterId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-class-module/{moduleClassId}")]
        [SwaggerOperation("Lấy thông tin lớp học phần")]
        public async Task<IActionResult> GetClassModuleAsync(string moduleClassId)
        {
            var response = await _classModuleServices.GetClassModuleAsync(moduleClassId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-students/{moduleClassId}")]
        [SwaggerOperation("Lấy danh sách sinh viên theo lớp học phần")]
        public async Task<IActionResult> GetStudentsByModuleClass(string moduleClassId)
        {
            var response = await _classModuleServices.GetStudentsByModuleClass(moduleClassId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("sync-students/{moduleClassId}")]
        [SwaggerOperation("Đồng bộ sinh viên vào lớp học phần")]
        public async Task<IActionResult> SyncClassModuleStudentsAsync(string moduleClassId)
        {
            var response = await _classModuleServices.SyncClassModuleStudentsAsync(moduleClassId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Authorize(Roles = AppRoles.Teacher)]
        [Route("add-student/{moduleClassId}")]
        [SwaggerOperation("Thêm sinh viên vào lớp học phần")]
        public async Task<IActionResult> AddStudentToClassAsync(string moduleClassId, [FromQuery] int studentId)
        {
            var response = await _classManagerServices.AddStudentToClassAsync(moduleClassId, studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Authorize(Roles = AppRoles.Teacher)]
        [Route("remove-student/{moduleClassId}")]
        [SwaggerOperation("Xóa sinh viên khỏi lớp học phần")]
        public async Task<IActionResult> RemoveStudentFromClassAsync(string moduleClassId, [FromQuery] int studentId)
        {
            var response = await _classManagerServices.RemoveStudentFromClassAsync(moduleClassId, studentId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-exam-seasons/{moduleClassId}")]
        [SwaggerOperation("Lấy danh sách kỳ thi theo lớp học phần")]
        public async Task<IActionResult> GetExamSeasonsByClassAsync(string moduleClassId, [FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _examSeasonServices.GetExamSeasonsByClassAsync(moduleClassId, pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
    }
}