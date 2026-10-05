using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs;
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

        // POST api/auth/register
        // Регистрация нового пользователя (доступна без авторизации)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var created = await _userService.RegisterAsync(dto);
            if (created == null)
                return Conflict(new { message = $"Пользователь с логином '{dto.Username}' уже существует" });

            return StatusCode(StatusCodes.Status201Created, created);
        }
    }
}