using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs;
using NewsApi.Services;
using NewsApi.Utils;

namespace NewsApi.Controllers
{
    [ApiController]
    [Route("api/news/{newsId}/comments")] // комментарии всегда идут вложенно в новость
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly INewsService _newsService;

        public CommentsController(ICommentService commentService, INewsService newsService)
        {
            _commentService = commentService;
            _newsService = newsService;
        }

        // GET api/news/5/comments
        // Просмотр доступен всем
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByNewsId(int newsId)
        {
            var comments = await _commentService.GetByNewsIdAsync(newsId);
            return Ok(comments);
        }

        // POST api/news/5/comments
        // Комментировать могут подписчики и администраторы
        [HttpPost]
        [Authorize(Roles = "admin,subscriber")]
        public async Task<IActionResult> Create(int newsId, [FromBody] CreateCommentDto dto)
        {
            // Нельзя оставить комментарий к новости, которой не существует
            var news = await _newsService.GetByIdAsync(newsId);
            if (news == null)
                return NotFound(new { message = $"Новость с id {newsId} не найдена" });

            var created = await _commentService.CreateAsync(newsId, dto, User.GetUserId());
            return CreatedAtAction(nameof(GetByNewsId), new { newsId }, created);
        }

        // PUT api/news/5/comments/12
        // Подписчик может менять только свои комментарии (проверка в сервисе), админ - любые
        [HttpPut("{id}")]
        [Authorize(Roles = "admin,subscriber")]
        public async Task<IActionResult> Update(int newsId, int id, [FromBody] UpdateCommentDto dto)
        {
            try
            {
                var updated = await _commentService.UpdateAsync(id, dto, User.GetUserId(), User.GetUserRole());
                if (!updated)
                    return NotFound(new { message = $"Комментарий с id {id} не найден" });

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // DELETE api/news/5/comments/12
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,subscriber")]
        public async Task<IActionResult> Delete(int newsId, int id)
        {
            try
            {
                var deleted = await _commentService.DeleteAsync(id, User.GetUserId(), User.GetUserRole());
                if (!deleted)
                    return NotFound(new { message = $"Комментарий с id {id} не найден" });

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}