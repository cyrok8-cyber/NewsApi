using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetByNewsIdAsync(int newsId);
        Task<Comment?> GetByIdAsync(int id);
        Task<Comment> CreateAsync(Comment comment);
        Task<bool> UpdateAsync(Comment comment);
        Task<bool> DeleteAsync(int id);
    }
}