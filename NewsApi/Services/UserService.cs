using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;
using NewsApi.Entities;
using NewsApi.Exceptions;
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

        public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
        {
            var existing = await _repository.GetByUsernameAsync(request.Username);
            if (existing != null)
                throw new ConflictException($"Пользователь с логином '{request.Username}' уже существует");

            var user = new User
            {
                Username = request.Username,
                Password = PasswordHasher.Hash(request.Password),
                Name = request.Name,
                Surname = request.Surname,
                ParentName = request.ParentName ?? string.Empty,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(user);

            return new UserResponse
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