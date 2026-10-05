using NewsApi.DTOs;

namespace NewsApi.Services
{
    public interface IUserService
    {
        // Возвращает null, если пользователь с таким username уже существует
        Task<UserDto?> RegisterAsync(RegisterUserDto dto);
    }
}