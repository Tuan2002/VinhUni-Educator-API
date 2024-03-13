using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VinhUni_Educator_API.Interfaces;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseServices _courseServices;
        public CoursesController(ICourseServices courseServices)
        {
            _courseServices = courseServices;
        }
        [HttpPost]
        [Route("sync")]
        public async Task<IActionResult> SyncCoursesAsync()
        {
            var response = await _courseServices.SyncCoursesAsync();
            return StatusCode(response.StatusCode, response);
        }

    }
}