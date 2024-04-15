
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
        public async Task<IActionResult> GetSchoolYears(int? pageIndex = DEFAULT_PAGE_INDEX, int? limit = DEFAULT_LIMIT)
        {
            var response = await _semesterServices.GetSchoolYearsAsync(pageIndex, limit);
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
        public async Task<IActionResult> GetSemestersBySchoolYear(int schoolYearId)
        {
            var response = await _semesterServices.GetSemestersBySchoolYearAsync(schoolYearId);
            return StatusCode(response.StatusCode, response);
        }
    }
}