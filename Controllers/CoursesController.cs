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
    [SwaggerTag("Quản lý danh mục khóa học")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseServices _courseServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public CoursesController(ICourseServices courseServices)
        {
            _courseServices = courseServices;
        }
        [HttpPost]
        [Route("sync")]
        [SwaggerOperation(Summary = "Đồng bộ danh sách khóa học", Description = "Đồng bộ danh sách khóa học từ hệ thống Đại học Vinh")]
        public async Task<IActionResult> SyncCoursesAsync()
        {
            var response = await _courseServices.SyncCoursesAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-courses")]
        [SwaggerOperation(Summary = "Lấy danh sách khóa học", Description = "Lấy danh sách khóa học")]
        public async Task<IActionResult> GetCoursesAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _courseServices.GetCoursesAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-courses")]
        [SwaggerOperation(Summary = "Lấy danh sách khóa học đã xóa", Description = "Lấy danh sách khóa học đã xóa")]
        public async Task<IActionResult> GetDeletedCoursesAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _courseServices.GetDeletedCoursesAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-course/{courseId}")]
        [SwaggerOperation(Summary = "Lấy thông tin khóa học", Description = "Lấy thông tin khóa học")]
        public async Task<IActionResult> GetCourseByIdAsync(int courseId)
        {
            var response = await _courseServices.GetCourseByIdAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-course/{courseId}")]
        [SwaggerOperation(Summary = "Xóa khóa học", Description = "Xóa khóa học")]
        public async Task<IActionResult> DeleteCourseAsync(int courseId)
        {
            var response = await _courseServices.DeleteCourseAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-course/{courseId}")]
        [SwaggerOperation(Summary = "Khôi phục khóa học", Description = "Khôi phục khóa học")]
        public async Task<IActionResult> RestoreCourseAsync(int courseId)
        {
            var response = await _courseServices.RestoreCourseAsync(courseId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-course/{courseId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin khóa học", Description = "Cập nhật thông tin khóa học")]
        public async Task<IActionResult> UpdateCourseAsync(int courseId, [FromBody] UpdateCourseModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _courseServices.UpdateCourseAsync(courseId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search")]
        [SwaggerOperation(Summary = "Tìm kiếm khóa học", Description = "Tìm kiếm khóa học")]
        public async Task<IActionResult> SearchCourseAsync([FromQuery] string? searchKey, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _courseServices.SearchCourseAsync(searchKey, limit);
            return StatusCode(response.StatusCode, response);
        }
    }

}