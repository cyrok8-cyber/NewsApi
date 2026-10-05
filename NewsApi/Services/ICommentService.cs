using NewsApi.DTOs;

namespace NewsApi.Services
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetByNewsIdAsync(int newsId);
        Task<CommentDto> CreateAsync(int newsId, CreateCommentDto dto, int userId);
        Task<bool> UpdateAsync(int id, UpdateCommentDto dto, int userId, string userRole);
        Task<bool> DeleteAsync(int id, int userId, string userRole);
    }
}