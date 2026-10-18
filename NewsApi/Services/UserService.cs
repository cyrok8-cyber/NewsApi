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
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
        {
            var existing = await _userRepository.GetByUsernameAsync(request.Username);
            if (existing is not null)
                throw new ConflictException("Пользователь с таким именем уже существует");

            var role = await _roleRepository.GetByNameAsync(request.Role);
            if (role is null)
                throw new NotFoundException($"Роль '{request.Role}' не найдена");

            var user = new User
            {
                Username = request.Username,
                Password = PasswordHasher.Hash(request.Password),
                Name = request.Name,
                Surname = request.Surname,
                ParentName = request.ParentName,
                RoleId = role.Id,
                Role = role,
                FailedLoginAttempts = 0,
                FailedBannedWordAttempts = 0,
                BlockedUntil = null
            };

            var created = await _userRepository.CreateAsync(user);
            return MapToResponse(created);
        }

        public async Task<UserResponse> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                throw new NotFoundException($"Пользователь с id={id} не найден");

            return MapToResponse(user);
        }

        public async Task BlockAsync(int id, BlockUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                throw new NotFoundException($"Пользователь с id={id} не найден");

            user.BlockedUntil = DateTime.UtcNow.AddMinutes(request.DurationMinutes);
            user.FailedLoginAttempts = 0;

            await _userRepository.UpdateAsync(user);
        }

        public async Task<UserResponse> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user is null)
                throw new AuthenticationFailedException("Неверный логин или пароль");

            if (user.BlockedUntil.HasValue && user.BlockedUntil.Value > DateTime.UtcNow)
                throw new AuthenticationFailedException($"Пользователь заблокирован до {user.BlockedUntil.Value:O}");

            var passwordOk = PasswordHasher.Verify(password, user.Password);

            if (!passwordOk)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= BlockingPolicy.MaxFailedLoginAttempts)
                {
                    user.BlockedUntil = DateTime.UtcNow.Add(BlockingPolicy.LoginBlockDuration);
                    user.FailedLoginAttempts = 0;
                }
                await _userRepository.UpdateAsync(user);
                throw new AuthenticationFailedException("Неверный логин или пароль");
            }

            if (user.FailedLoginAttempts != 0 || user.BlockedUntil.HasValue)
            {
                user.FailedLoginAttempts = 0;
                user.BlockedUntil = null;
                await _userRepository.UpdateAsync(user);
            }

            return MapToResponse(user);
        }

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Surname = user.Surname,
                ParentName = user.ParentName,
                Role = user.Role.Name,
                IsBlocked = user.BlockedUntil.HasValue && user.BlockedUntil.Value > DateTime.UtcNow,
                BlockedUntil = user.BlockedUntil
            };
        }
    }
}