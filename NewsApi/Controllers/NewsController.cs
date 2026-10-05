using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs;
using NewsApi.Services;
using NewsApi.Utils;

namespace NewsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly ICommentService _commentService;

        public NewsController(INewsService newsService, ICommentService commentService)
        {
            _newsService = newsService;
            _commentService = commentService;
        }

        // GET api/news?page=1&pageSize=10
        // Просмотр доступен всем, в том числе незарегистрированным
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _newsService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        // GET api/news/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var news = await _newsService.GetByIdAsync(id);
            if (news == null)
                return NotFound(new { message = $"Новость с id {id} не найдена" });

            return Ok(news);
        }

        // GET api/news/5/with-comments
        // Новость вместе со всеми её комментариями
        [HttpGet("{id}/with-comments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWithComments(int id)
        {
            var news = await _newsService.GetByIdAsync(id);
            if (news == null)
                return NotFound(new { message = $"Новость с id {id} не найдена" });

            var comments = await _commentService.GetByNewsIdAsync(id);

            var result = new NewsWithCommentsDto
            {
                News = news,
                Comments = comments
            };

            return Ok(result);
        }

        // POST api/news
        // Создавать новости могут журналисты и администраторы
        [HttpPost]
        [Authorize(Roles = "admin,reporter")]
        public async Task<IActionResult> Create([FromBody] CreateNewsDto dto)
        {
            var created = await _newsService.CreateAsync(dto, User.GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/news/5
        // Журналист может менять только свои новости (проверка в сервисе), админ - любые
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,reporter")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNewsDto dto)
        {
            try
            {
                var updated = await _newsService.UpdateAsync(id, dto, User.GetUserId(), User.GetUserRole());
                if (!updated)
                    return NotFound(new { message = $"Новость с id {id} не найдена" });

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE api/news/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,reporter")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _newsService.DeleteAsync(id, User.GetUserId(), User.GetUserRole());
                if (!deleted)
                    return NotFound(new { message = $"Новость с id {id} не найдена" });

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}