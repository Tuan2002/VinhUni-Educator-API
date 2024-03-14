using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Quản lý chương trình đào tạo")]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramServices _programServices;
        public ProgramsController(IProgramServices programServices)
        {
            _programServices = programServices;
        }
        [HttpPost]
        [Route("sync")]
        [SwaggerOperation(Summary = "Đồng bộ chương trình đào tạo từ hệ thống USmart")]
        public async Task<IActionResult> SyncProgramsAsync()
        {
            var response = await _programServices.SyncProgramsAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}