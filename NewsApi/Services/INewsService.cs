using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;

namespace NewsApi.Services
{
    public interface INewsService
    {
        Task<PagedResponse<NewsResponse>> GetAllAsync(int page, int pageSize);
        Task<NewsResponse> GetByIdAsync(int id);
        Task<NewsResponse> CreateAsync(CreateNewsRequest request, int userId);
        Task UpdateAsync(int id, UpdateNewsRequest request, int userId, string userRole);
        Task DeleteAsync(int id, int userId, string userRole);
    }
}