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
    [SwaggerTag("Quản lý cơ cấu tổ chức")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationServices _organizationServices;
        private const int DEFAULT_PAGE_INDEX = 1;
        private const int DEFAULT_LIMIT = 10;
        private const int DEFAULT_LIMIT_SEARCH = 10;
        public OrganizationsController(IOrganizationServices organizationServices)
        {
            _organizationServices = organizationServices;
        }

        [HttpPost]
        [Route("sync")]
        [SwaggerOperation(Summary = "Đồng bộ danh sách đơn vị, phòng ban", Description = "Đồng bộ danh sách phòng ban từ hệ thống Đại học Vinh")]
        public async Task<IActionResult> SyncOrganizations()
        {
            var response = await _organizationServices.SyncOrganizationsAsync();
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-organizations")]
        [SwaggerOperation(Summary = "Lấy danh sách đơn vị, phòng ban", Description = "Lấy danh sách phòng ban từ hệ thống")]
        public async Task<IActionResult> GetOrganizations([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _organizationServices.GetOrganizationsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-organization/{organizationId}")]
        [SwaggerOperation(Summary = "Lấy thông tin đơn vị, phòng ban", Description = "Lấy thông tin phòng ban từ hệ thống")]
        public async Task<IActionResult> GetOrganizationById(int organizationId)
        {
            var response = await _organizationServices.GetOrganizationByIdAsync(organizationId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("get-deleted-organizations")]
        [SwaggerOperation(Summary = "Lấy danh sách đơn vị, phòng ban đã xóa", Description = "Lấy danh sách phòng ban đã xóa khỏi hệ thống")]
        public async Task<IActionResult> GetDeletedOrganizations([FromQuery] int? pageIndex = DEFAULT_PAGE_INDEX, [FromQuery] int? limit = DEFAULT_LIMIT)
        {
            var response = await _organizationServices.GetDeletedOrganizationsAsync(pageIndex, limit);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete]
        [Route("delete-organization/{organizationId}")]
        [SwaggerOperation(Summary = "Xóa đơn vị, phòng ban", Description = "Xóa phòng ban khỏi hệ thống")]
        public async Task<IActionResult> DeleteOrganization(int organizationId)
        {
            var response = await _organizationServices.DeleteOrganizationAsync(organizationId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("update-organization/{organizationId}")]
        [SwaggerOperation(Summary = "Cập nhật thông tin đơn vị, phòng ban", Description = "Cập nhật thông tin phòng ban trong hệ thống")]
        public async Task<IActionResult> UpdateOrganization(int organizationId, [FromBody] UpdateOrganizationModel model)
        {
            var response = await _organizationServices.UpdateOrganizationAsync(organizationId, model);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut]
        [Route("restore-organization/{organizationId}")]
        [SwaggerOperation(Summary = "Khôi phục đơn vị, phòng ban", Description = "Khôi phục phòng ban đã xóa khỏi hệ thống")]
        public async Task<IActionResult> RestoreOrganization(int organizationId)
        {
            var response = await _organizationServices.RestoreOrganizationAsync(organizationId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("search-organizations")]
        [SwaggerOperation(Summary = "Tìm kiếm đơn vị, phòng ban", Description = "Tìm kiếm phòng ban trong hệ thống")]
        public async Task<IActionResult> SearchOrganizations([FromQuery] string? searchKey, [FromQuery] int? limit = DEFAULT_LIMIT_SEARCH)
        {
            var response = await _organizationServices.SearchOrganizationsAsync(searchKey, limit);
            return StatusCode(response.StatusCode, response);
        }
    }
}