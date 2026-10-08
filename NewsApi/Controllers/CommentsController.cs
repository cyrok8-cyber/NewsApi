using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs.Requests;
using NewsApi.Services;
using NewsApi.Utils;

namespace NewsApi.Controllers
{
    [ApiController]
    [Route("api/news/{newsId}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByNewsId(int newsId)
        {
            var comments = await _commentService.GetByNewsIdAsync(newsId);
            return Ok(comments);
        }

        [HttpPost]
        [Authorize(Roles = "admin,subscriber")]
        public async Task<IActionResult> Create(int newsId, [FromBody] CreateCommentRequest request)
        {
            var created = await _commentService.CreateAsync(newsId, request, User.GetUserId());
            return CreatedAtAction(nameof(GetByNewsId), new { newsId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,subscriber")]
        public async Task<IActionResult> Update(int newsId, int id, [FromBody] UpdateCommentRequest request)
        {
            await _commentService.UpdateAsync(id, request, User.GetUserId(), User.GetUserRole());
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,subscriber")]
        public async Task<IActionResult> Delete(int newsId, int id)
        {
            await _commentService.DeleteAsync(id, User.GetUserId(), User.GetUserRole());
            return NoContent();
        }
    }
}