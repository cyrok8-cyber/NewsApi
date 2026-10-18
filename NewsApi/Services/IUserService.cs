using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;

namespace NewsApi.Services
{
    public interface IUserService
    {
        Task<UserResponse> RegisterAsync(RegisterUserRequest request);
        Task<UserResponse> GetByIdAsync(int id);
        Task BlockAsync(int id, BlockUserRequest request);
        Task<UserResponse> AuthenticateAsync(string username, string password);
    }
}