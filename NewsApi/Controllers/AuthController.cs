using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs.Requests;
using NewsApi.Services;

namespace NewsApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var created = await _userService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, created);
        }
    }
}