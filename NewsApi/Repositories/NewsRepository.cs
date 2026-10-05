using Microsoft.EntityFrameworkCore;
using NewsApi.Data;
using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public class NewsRepository : INewsRepository
    {
        private readonly AppDbContext _context;

        public NewsRepository(AppDbContext context)
        {
            _context = context;
        }

        // Получаем список новостей постранично (пагинация)
        public async Task<List<News>> GetAllAsync(int page, int pageSize)
        {
            return await _context.News
                .OrderByDescending(n => n.CreatedAt) // сначала свежие
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Общее количество новостей - нужно, чтобы посчитать сколько всего страниц
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.News.CountAsync();
        }

        public async Task<News?> GetByIdAsync(int id)
        {
            return await _context.News.FindAsync(id);
        }

        public async Task<News> CreateAsync(News news)
        {
            _context.News.Add(news);
            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<bool> UpdateAsync(News news)
        {
            var existing = await _context.News.FindAsync(news.Id);
            if (existing == null) return false;

            existing.Title = news.Title;
            existing.Text = news.Text;
            existing.EditedAt = DateTime.UtcNow;
            existing.UpdatedBy = news.UpdatedBy;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.News.FindAsync(id);
            if (existing == null) return false;

            _context.News.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}