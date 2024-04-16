using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [SwaggerTag("Các chức năng liên quan đến học kỳ, năm học")]
    public class SemestersController : ControllerBase
    {
        private readonly ISemesterServices _semesterServices;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_PAGE_INDEX = 1;
        public SemestersController(ISemesterServices semesterServices)
        {
            _semesterServices = semesterServices;
        }
        [HttpPost]
        [Route("sync-school-years")]
        [SwaggerOperation(Summary = "Đồng bộ năm học từ hệ thống USmart", Description = "Đồng bộ năm học từ hệ thống USmart")]
        public async Task<IActionResult> SyncSchoolYears()
        {
            var response = await _semesterServices.SyncSchoolYearsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        [Route("sync-semesters")]
        [SwaggerOperation(Summary = "Đồng bộ học kỳ từ hệ thống USmart", Description = "Đồng bộ học kỳ từ hệ thống USmart")]
        public async Task<IActionResult> SyncSemesters()
        {
            var response = await _semesterServices.SyncSemestersAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("get-school-years")]
        [SwaggerOperation(Summary = "Lấy danh sách năm học", Description = "Lấy danh sách năm học")]
        public async Task<IActionResult> GetSchoolYears(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT, bool skipCache = false)
        {
            var response = await _semesterServices.GetSchoolYearsAsync(pageIndex, limit, skipCache);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("get-semesters")]
        [SwaggerOperation(Summary = "Lấy danh sách học kỳ", Description = "Lấy danh sách học kỳ")]
        public async Task<IActionResult> GetSemesters(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _semesterServices.GetSemestersAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("get-semesters-by-year/{schoolYearId}")]
        [SwaggerOperation(Summary = "Lấy danh sách học kỳ theo năm học", Description = "Lấy danh sách học kỳ theo năm học")]
        public async Task<IActionResult> GetSemestersBySchoolYear(int schoolYearId, bool skipCache = false)
        {
            var response = await _semesterServices.GetSemestersBySchoolYearAsync(schoolYearId, skipCache);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("get-semester/{semesterId}")]
        [SwaggerOperation(Summary = "Lấy thông tin học kỳ", Description = "Lấy thông tin học kỳ")]
        public async Task<IActionResult> GetSemesterById(int semesterId)
        {
            var response = await _semesterServices.GetSemesterByIdAsync(semesterId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-school-year/{schoolYearId}")]
        [SwaggerOperation(Summary = "Xóa năm học", Description = "Xóa năm học")]
        public async Task<IActionResult> DeleteSchoolYear(int schoolYearId)
        {
            var response = await _semesterServices.DeleteSchoolYearAsync(schoolYearId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-semester/{semesterId}")]
        [SwaggerOperation(Summary = "Xóa học kỳ", Description = "Xóa học kỳ")]
        public async Task<IActionResult> DeleteSemester(int semesterId)
        {
            var response = await _semesterServices.DeleteSemesterAsync(semesterId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-school-year/{schoolYearId}")]
        [SwaggerOperation(Summary = "Khôi phục năm học", Description = "Khôi phục năm học")]
        public async Task<IActionResult> RestoreSchoolYear(int schoolYearId)
        {
            var response = await _semesterServices.RestoreSchoolYearAsync(schoolYearId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-semester/{semesterId}")]
        [SwaggerOperation(Summary = "Khôi phục học kỳ", Description = "Khôi phục học kỳ")]
        public async Task<IActionResult> RestoreSemester(int semesterId)
        {
            var response = await _semesterServices.RestoreSemesterAsync(semesterId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-school-years")]
        [SwaggerOperation(Summary = "Lấy danh sách năm học đã xóa", Description = "Lấy danh sách năm học đã xóa")]
        public async Task<IActionResult> GetDeletedSchoolYears(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _semesterServices.GetDeletedSchoolYearsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-semesters")]
        [SwaggerOperation(Summary = "Lấy danh sách học kỳ đã xóa", Description = "Lấy danh sách học kỳ đã xóa")]
        public async Task<IActionResult> GetDeletedSemesters(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _semesterServices.GetDeletedSemestersAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
    }
}