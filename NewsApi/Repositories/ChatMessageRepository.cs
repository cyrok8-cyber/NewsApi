using Microsoft.EntityFrameworkCore;
using NewsApi.Data;
using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly AppDbContext _context;

        public ChatMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessage>> GetByOfficeAsync(int officeId, int page, int pageSize)
        {
            return await _context.ChatMessages
                .Include(m => m.Author)
                .Where(m => m.EditorialOfficeId == officeId)
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountByOfficeAsync(int officeId)
        {
            return await _context.ChatMessages.CountAsync(m => m.EditorialOfficeId == officeId);
        }

        public async Task<ChatMessage> CreateAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // Подгружаем автора, чтобы сразу вернуть его ФИО в ответе
            await _context.Entry(message).Reference(m => m.Author).LoadAsync();
            return message;
        }
    }
}
