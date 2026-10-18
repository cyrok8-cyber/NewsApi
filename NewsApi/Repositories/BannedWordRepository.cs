using Microsoft.EntityFrameworkCore;
using NewsApi.Data;

namespace NewsApi.Repositories
{
    public class BannedWordRepository : IBannedWordRepository
    {
        private readonly AppDbContext _context;

        public BannedWordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ContainsBannedWordAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            var words = await _context.BannedWords.Select(w => w.Word).ToListAsync();
            return words.Any(w => text.Contains(w, StringComparison.OrdinalIgnoreCase));
        }
    }
}