using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs.Requests;
using NewsApi.Services;
using NewsApi.Services.Facades;
using NewsApi.Utils;

namespace NewsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly INewsCommentsFacade _newsCommentsFacade;

        public NewsController(INewsService newsService, INewsCommentsFacade newsCommentsFacade)
        {
            _newsService = newsService;
            _newsCommentsFacade = newsCommentsFacade;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _newsService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var news = await _newsService.GetByIdAsync(id);
            return Ok(news);
        }

        [HttpGet("{id}/with-comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWithComments(int id)
        {
            var result = await _newsCommentsFacade.GetNewsWithCommentsAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "admin,reporter")]
        public async Task<IActionResult> Create([FromBody] CreateNewsRequest request)
        {
            var created = await _newsService.CreateAsync(request, User.GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,reporter")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNewsRequest request)
        {
            await _newsService.UpdateAsync(id, request, User.GetUserId(), User.GetUserRole());
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,reporter")]
        public async Task<IActionResult> Delete(int id)
        {
            await _newsService.DeleteAsync(id, User.GetUserId(), User.GetUserRole());
            return NoContent();
        }
    }
}