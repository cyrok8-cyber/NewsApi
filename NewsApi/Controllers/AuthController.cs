using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userService.AuthenticateAsync(request.Username, request.Password);

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new
            {
                message = "Вход выполнен успешно",
                user.Id,
                user.Username,
                user.Role
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            Response.Headers["WWW-Authenticate"] = "Basic realm=\"NewsApi\", charset=\"UTF-8\"";
            return StatusCode(401, new
            {
                message = "Вы вышли из системы. Чтобы логин/пароль больше не отправлялись автоматически, закройте вкладку браузера или очистите сохранённые учётные данные."
            });
        }
    }
}