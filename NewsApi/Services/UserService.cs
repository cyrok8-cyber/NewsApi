using NewsApi.DTOs;
using NewsApi.Entities;
using NewsApi.Repositories;
using NewsApi.Utils;

namespace NewsApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserDto?> RegisterAsync(RegisterUserDto dto)
        {
            // Логин должен быть уникальным
            var existing = await _repository.GetByUsernameAsync(dto.Username);
            if (existing != null)
                return null;

            var user = new User
            {
                Username = dto.Username,
                Password = PasswordHasher.Hash(dto.Password), // в базу попадает только хеш
                Name = dto.Name,
                Surname = dto.Surname,
                ParentName = dto.ParentName ?? string.Empty,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(user);

            return new UserDto
            {
                Id = created.Id,
                Username = created.Username,
                Name = created.Name,
                Surname = created.Surname,
                ParentName = created.ParentName,
                Role = created.Role,
                CreatedAt = created.CreatedAt,
                EditedAt = created.EditedAt
            };
        }
    }
}