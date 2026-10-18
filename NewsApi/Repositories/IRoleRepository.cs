using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name);
    }
}