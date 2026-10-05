using NewsApi.DTOs;

namespace NewsApi.Services
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public interface INewsService
    {
        Task<PagedResult<NewsDto>> GetAllAsync(int page, int pageSize);
        Task<NewsDto?> GetByIdAsync(int id);
        Task<NewsDto> CreateAsync(CreateNewsDto dto, int userId);
        Task<bool> UpdateAsync(int id, UpdateNewsDto dto, int userId, string userRole);
        Task<bool> DeleteAsync(int id, int userId, string userRole);
    }
}