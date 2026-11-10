using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs.Requests;
using NewsApi.Services;

namespace NewsApi.Controllers
{
    // Редакции: просматривать могут все, создавать/изменять/удалять
    // и управлять составом - только администратор
    [ApiController]
    [Route("api/editorial-offices")]
    public class EditorialOfficesController : ControllerBase
    {
        private readonly IEditorialOfficeService _officeService;

        public EditorialOfficesController(IEditorialOfficeService officeService)
        {
            _officeService = officeService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await _officeService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _officeService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] CreateEditorialOfficeRequest request)
        {
            var created = await _officeService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEditorialOfficeRequest request)
        {
            await _officeService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _officeService.DeleteAsync(id);
            return NoContent();
        }

        // Добавить журналиста в редакцию
        [HttpPost("{id}/members/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddMember(int id, int userId)
        {
            await _officeService.AddMemberAsync(id, userId);
            return NoContent();
        }

        // Исключить журналиста из редакции
        [HttpDelete("{id}/members/{userId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveMember(int id, int userId)
        {
            await _officeService.RemoveMemberAsync(id, userId);
            return NoContent();
        }
    }
}
