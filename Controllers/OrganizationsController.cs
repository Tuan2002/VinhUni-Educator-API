using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý cơ cấu tổ chức")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationServices _organizationServices;
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
    }
}