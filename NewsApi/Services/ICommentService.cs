using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;

namespace NewsApi.Services
{
    public interface ICommentService
    {
        Task<List<CommentResponse>> GetByNewsIdAsync(int newsId);
        Task<CommentResponse> CreateAsync(int newsId, CreateCommentRequest request, int userId);
        Task UpdateAsync(int id, UpdateCommentRequest request, int userId, string userRole);
        Task DeleteAsync(int id, int userId, string userRole);
    }
}