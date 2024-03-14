using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Quản lý danh mục khóa học")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseServices _courseServices;
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

    }
}