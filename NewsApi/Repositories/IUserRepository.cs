using NewsApi.Entities;

namespace NewsApi.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<User> CreateAsync(User user);
    }
}