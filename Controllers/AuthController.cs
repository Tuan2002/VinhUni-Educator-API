using Microsoft.AspNetCore.Mvc;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Models;
using VinhUni_Educator_API.Utils;

namespace VinhUni_Educator_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var response = await _authServices.LoginAsync(model);
            if (!response.IsSuccess)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
        [HttpPost]
        [Route("login-sso")]
        public async Task<IActionResult> LogiSSO([FromBody] LoginModel model)
        {
            var response = await _authServices.LoginSSOAsync(model);
            if (!response.IsSuccess)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }

    }
}