using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public interface IEditorialOfficeRepository
    {
        Task<List<EditorialOffice>> GetAllAsync();
        Task<EditorialOffice?> GetByIdAsync(int id);
        Task<EditorialOffice?> GetByNameAsync(string name);
        Task<EditorialOffice> CreateAsync(EditorialOffice office);
        Task UpdateAsync(EditorialOffice office);
        Task DeleteAsync(EditorialOffice office);
    }
}
