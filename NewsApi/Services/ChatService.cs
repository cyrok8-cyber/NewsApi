using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services.Censorship;

namespace NewsApi.Services
{
    // Внутренний чат редакции.
    // Писать и читать могут только журналисты этой редакции,
    // администратор может читать любой чат (модерация), но не пишет в него.
    // Каждое сообщение перед сохранением проходит цепочку цензуры (Chain of Responsibility)
    public class ChatService : IChatService
    {
        private const string AdminRole = "admin";

        private readonly IChatMessageRepository _chatRepository;
        private readonly IEditorialOfficeRepository _officeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatCensorshipChain _censorship;

        public ChatService(
            IChatMessageRepository chatRepository,
            IEditorialOfficeRepository officeRepository,
            IUserRepository userRepository,
            IChatCensorshipChain censorship)
        {
            _chatRepository = chatRepository;
            _officeRepository = officeRepository;
            _userRepository = userRepository;
            _censorship = censorship;
        }

        public async Task<PagedResponse<ChatMessageResponse>> GetMessagesAsync(
            int officeId, int userId, string role, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            await EnsureOfficeExistsAsync(officeId);

            if (role != AdminRole)
                await EnsureMemberAsync(officeId, userId);

            var messages = await _chatRepository.GetByOfficeAsync(officeId, page, pageSize);
            var total = await _chatRepository.CountByOfficeAsync(officeId);

            // Из БД страница приходит от новых к старым, а читать чат удобнее сверху вниз
            messages.Reverse();

            return new PagedResponse<ChatMessageResponse>
            {
                Items = messages.Select(MapToResponse).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ChatMessageResponse> SendMessageAsync(
            int officeId, int userId, string role, SendChatMessageRequest request)
        {
            await EnsureOfficeExistsAsync(officeId);
            await EnsureMemberAsync(officeId, userId); // администратор в чат не пишет

            var censored = await _censorship.ProcessAsync(request.Text);

            var message = new ChatMessage
            {
                EditorialOfficeId = officeId,
                AuthorId = userId,
                Text = censored.Text,
                WasCensored = censored.WasCensored,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _chatRepository.CreateAsync(message);
            return MapToResponse(created);
        }

        private async Task EnsureOfficeExistsAsync(int officeId)
        {
            if (await _officeRepository.GetByIdAsync(officeId) is null)
                throw new NotFoundException($"Редакция с id={officeId} не найдена");
        }

        private async Task EnsureMemberAsync(int officeId, int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null || user.EditorialOfficeId != officeId)
                throw new UnauthorizedAccessException("Чат доступен только журналистам этой редакции");
        }

        private static ChatMessageResponse MapToResponse(ChatMessage message)
        {
            return new ChatMessageResponse
            {
                Id = message.Id,
                Text = message.Text,
                CreatedAt = message.CreatedAt,
                WasCensored = message.WasCensored,
                Author = new UserShortResponse
                {
                    Id = message.AuthorId,
                    Surname = message.Author?.Surname ?? string.Empty,
                    Name = message.Author?.Name ?? string.Empty,
                    ParentName = message.Author?.ParentName ?? string.Empty
                }
            };
        }
    }
}
