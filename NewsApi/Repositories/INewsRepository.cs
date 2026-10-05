using NewsApi.Entities;

namespace NewsApi.Repositories
{
    // Описываем, что должен уметь делать репозиторий новостей
    public interface INewsRepository
    {
        Task<List<News>> GetAllAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
        Task<News?> GetByIdAsync(int id);
        Task<News> CreateAsync(News news);
        Task<bool> UpdateAsync(News news);
        Task<bool> DeleteAsync(int id);
    }
}