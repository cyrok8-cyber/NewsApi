using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApi.DTOs.Requests;
using NewsApi.Services;
using NewsApi.Utils;

namespace NewsApi.Controllers
{
    // Внутренний чат редакции
    [ApiController]
    [Route("api/editorial-offices/{officeId}/chat")]
    public class EditorialOfficeChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public EditorialOfficeChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Сообщения чата: page=1 - самые свежие, внутри страницы - от старых к новым
        [HttpGet]
        [Authorize(Roles = "admin,reporter")]
        public async Task<IActionResult> GetMessages(int officeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _chatService.GetMessagesAsync(
                officeId, User.GetUserId(), User.GetUserRole(), page, pageSize);
            return Ok(result);
        }

        // Отправить сообщение (только журналисты этой редакции)
        [HttpPost]
        [Authorize(Roles = "reporter")]
        public async Task<IActionResult> SendMessage(int officeId, [FromBody] SendChatMessageRequest request)
        {
            var result = await _chatService.SendMessageAsync(
                officeId, User.GetUserId(), User.GetUserRole(), request);
            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
