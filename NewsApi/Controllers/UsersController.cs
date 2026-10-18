using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs.Requests;
using NewsApi.Services;

namespace NewsApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _userService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/block")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Block(int id, [FromBody] BlockUserRequest request)
        {
            await _userService.BlockAsync(id, request);
            return NoContent();
        }
    }
}