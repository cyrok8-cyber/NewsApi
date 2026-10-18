using Microsoft.EntityFrameworkCore;
using NewsApi.Data;
using NewsApi.Entities;
using NewsApi.Utils;

namespace NewsApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task RegisterFailedBannedWordAttemptAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return;

            user.FailedBannedWordAttempts++;
            if (user.FailedBannedWordAttempts >= BlockingPolicy.MaxFailedBannedWordAttempts)
            {
                user.BlockedUntil = DateTime.UtcNow.Add(BlockingPolicy.BannedWordBlockDuration);
                user.FailedBannedWordAttempts = 0;
            }

            await _context.SaveChangesAsync();
        }
    }
}