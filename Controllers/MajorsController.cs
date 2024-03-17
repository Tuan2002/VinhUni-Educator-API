using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [SwaggerTag("Quản lý danh mục ngành học")]
    public class MajorsController : ControllerBase
    {
        private readonly IMajorServices _majorServices;
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
    }
}