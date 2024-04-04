using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;

namespace VinhUni_Educator_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Quản lý danh mục ngành học")]
    public class MajorsController : ControllerBase
    {
        private readonly IMajorServices _majorServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public MajorsController(IMajorServices majorServices)
        {
            _majorServices = majorServices;
        }

        [HttpPost]
        [Route("sync")]
        [SwaggerOperation(Summary = "Đồng bộ danh sách ngành học", Description = "Đồng bộ danh sách ngành học từ hệ thống Đại học Vinh")]
        public async Task<IActionResult> SyncMajorAsync()
        {
            var response = await _majorServices.SyncMajorAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-majors")]
        [SwaggerOperation(Summary = "Lấy danh sách ngành học", Description = "Lấy danh sách ngành học từ hệ thống")]
        public async Task<IActionResult> GetMajorsAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _majorServices.GetMajorsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-majors")]
        [SwaggerOperation(Summary = "Lấy danh sách ngành học đã xóa", Description = "Lấy danh sách ngành học đã xóa khỏi hệ thống")]
        public async Task<IActionResult> GetDeletedMajorsAsync([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _majorServices.GetDeletedMajorsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-major/{majorId}")]
        [SwaggerOperation(Summary = "Lấy thông tin ngành học", Description = "Lấy thông tin ngành học từ hệ thống")]
        public async Task<IActionResult> GetMajorByIdAsync(int majorId)
        {
            var response = await _majorServices.GetMajorByIdAsync(majorId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-major/{majorId}")]
        [SwaggerOperation(Summary = "Xóa ngành học", Description = "Xóa ngành học khỏi hệ thống")]
        public async Task<IActionResult> DeleteMajorAsync(int majorId)
        {
            var response = await _majorServices.DeleteMajorAsync(majorId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-major/{majorId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin ngành học", Description = "Cập nhật thông tin ngành học trong hệ thống")]
        public async Task<IActionResult> UpdateMajorAsync(int majorId, [FromBody] UpdateMajorModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await _majorServices.UpdateMajorAsync(majorId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-major/{majorId}")]
        [SwaggerOperation(Summary = "Khôi phục ngành học", Description = "Khôi phục ngành học đã bị xóa")]
        public async Task<IActionResult> RestoreMajorAsync(int majorId)
        {
            var response = await _majorServices.RestoreMajorAsync(majorId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search")]
        [SwaggerOperation(Summary = "Tìm kiếm ngành học", Description = "Tìm kiếm ngành học theo từ khóa")]
        public async Task<IActionResult> SearchMajorsAsync([FromQuery] string searchKey, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _majorServices.SearchMajorsAsync(searchKey, limit);
            return StatusCode(response.StatusCode, response);

        }
    }
}