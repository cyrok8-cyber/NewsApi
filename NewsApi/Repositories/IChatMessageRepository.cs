using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public interface IChatMessageRepository
    {
        // Страница сообщений редакции: page=1 - самые свежие
        Task<List<ChatMessage>> GetByOfficeAsync(int officeId, int page, int pageSize);
        Task<int> CountByOfficeAsync(int officeId);
        Task<ChatMessage> CreateAsync(ChatMessage message);
    }
}
