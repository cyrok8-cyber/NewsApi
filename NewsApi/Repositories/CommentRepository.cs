using Microsoft.EntityFrameworkCore;
using NewsApi.Data;
using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Все комментарии, относящиеся к конкретной новости
        public async Task<List<Comment>> GetByNewsIdAsync(int newsId)
        {
            return await _context.Comments
                .Where(c => c.NewsId == newsId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> UpdateAsync(Comment comment)
        {
            var existing = await _context.Comments.FindAsync(comment.Id);
            if (existing == null) return false;

            existing.Text = comment.Text;
            existing.EditedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Comments.FindAsync(id);
            if (existing == null) return false;

            _context.Comments.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}