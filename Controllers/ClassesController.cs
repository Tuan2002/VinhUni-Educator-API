using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Quản lý danh mục lớp hành chính")]
    public class ClassesController : ControllerBase
    {
        private readonly IPrimaryClassServices _primaryClassServices;
        public ClassesController(IPrimaryClassServices primaryClassServices)
        {
            _primaryClassServices = primaryClassServices;
        }

        [HttpPost]
        [Route("sync")]
        [SwaggerOperation(Summary = "Đồng bộ danh sách lớp hành chính", Description = "Đồng bộ danh sách lớp hành chính từ hệ thống Đại học Vinh")]
        public async Task<IActionResult> SyncPrimaryClasses()
        {
            var response = await _primaryClassServices.SyncPrimaryClassesAsync();
            return StatusCode(response.StatusCode, response);
        }

    }
}