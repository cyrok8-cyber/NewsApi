using Moq;
using NewsApi.DTOs.Requests;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services;
using NewsApi.Utils;
using Xunit;

namespace NewsApi.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _service = new UserService(_userRepositoryMock.Object, _roleRepositoryMock.Object);
        }

        private static User CreateUser(string username, string plainPassword, int failedAttempts = 0, DateTime? blockedUntil = null)
        {
            return new User
            {
                Id = 1,
                Username = username,
                Password = PasswordHasher.Hash(plainPassword),
                Name = "Иван",
                Surname = "Иванов",
                ParentName = "Иванович",
                RoleId = 1,
                Role = new Role { Id = 1, Name = "admin" },
                FailedLoginAttempts = failedAttempts,
                BlockedUntil = blockedUntil
            };
        }

        [Fact]
        public async Task AuthenticateAsync_ValidCredentials_ReturnsUserResponse()
        {
            var user = CreateUser("admin", "admin123");
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("admin"))
                .ReturnsAsync(user);

            var result = await _service.AuthenticateAsync("admin", "admin123");

            Assert.Equal("admin", result.Username);
            Assert.Equal("admin", result.Role);
            Assert.False(result.IsBlocked);
        }

        [Fact]
        public async Task AuthenticateAsync_WrongPassword_ThrowsAuthenticationFailedException()
        {
            var user = CreateUser("admin", "admin123");
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("admin"))
                .ReturnsAsync(user);

            await Assert.ThrowsAsync<AuthenticationFailedException>(
                () => _service.AuthenticateAsync("admin", "wrongpassword"));
        }

        [Fact]
        public async Task AuthenticateAsync_UnknownUser_ThrowsAuthenticationFailedException()
        {
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("ghost"))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<AuthenticationFailedException>(
                () => _service.AuthenticateAsync("ghost", "anything"));
        }

        [Fact]
        public async Task AuthenticateAsync_UserBlocked_ThrowsAuthenticationFailedException_EvenWithCorrectPassword()
        {
            var user = CreateUser("admin", "admin123", blockedUntil: DateTime.UtcNow.AddMinutes(10));
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("admin"))
                .ReturnsAsync(user);

            await Assert.ThrowsAsync<AuthenticationFailedException>(
                () => _service.AuthenticateAsync("admin", "admin123"));
        }

        [Fact]
        public async Task AuthenticateAsync_FifthFailedAttempt_SetsBlockedUntil()
        {
            var user = CreateUser("admin", "admin123", failedAttempts: 4);
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("admin"))
                .ReturnsAsync(user);

            await Assert.ThrowsAsync<AuthenticationFailedException>(
                () => _service.AuthenticateAsync("admin", "wrongpassword"));

            Assert.NotNull(user.BlockedUntil);
            Assert.True(user.BlockedUntil > DateTime.UtcNow);
            Assert.Equal(0, user.FailedLoginAttempts);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ThrowsConflictException()
        {
            var existing = CreateUser("admin", "admin123");
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("admin"))
                .ReturnsAsync(existing);

            var request = new RegisterUserRequest
            {
                Username = "admin",
                Password = "newpass",
                Name = "Пётр",
                Surname = "Петров",
                ParentName = "Петрович",
                Role = "subscriber"
            };

            await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_UnknownRole_ThrowsNotFoundException()
        {
            _userRepositoryMock.Setup(r => r.GetByUsernameAsync("newuser"))
                .ReturnsAsync((User?)null);
            _roleRepositoryMock.Setup(r => r.GetByNameAsync("ghostrole"))
                .ReturnsAsync((Role?)null);

            var request = new RegisterUserRequest
            {
                Username = "newuser",
                Password = "pass123",
                Name = "Пётр",
                Surname = "Петров",
                ParentName = "Петрович",
                Role = "ghostrole"
            };

            await Assert.ThrowsAsync<NotFoundException>(() => _service.RegisterAsync(request));
        }
    }
}