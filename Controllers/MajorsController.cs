using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MajorsController : ControllerBase
    {
        private readonly IMajorServices _majorServices;
        public MajorsController(IMajorServices majorServices)
        {
            _majorServices = majorServices;
        }

        [HttpGet("sync")]
        public async Task<IActionResult> SyncMajorAsync()
        {
            var response = await _majorServices.SyncMajorAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}