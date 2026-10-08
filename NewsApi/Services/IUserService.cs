using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;

namespace NewsApi.Services
{
    public interface IUserService
    {
        Task<UserResponse> RegisterAsync(RegisterUserRequest request);
    }
}