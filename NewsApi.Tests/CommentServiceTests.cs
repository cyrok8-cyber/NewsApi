using Moq;
using NewsApi.DTOs.Requests;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services;
using Xunit;

namespace NewsApi.Tests
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<INewsRepository> _newsRepositoryMock;
        private readonly Mock<IBannedWordRepository> _bannedWordRepositoryMock;
        private readonly CommentService _service;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _newsRepositoryMock = new Mock<INewsRepository>();
            _bannedWordRepositoryMock = new Mock<IBannedWordRepository>();
            _service = new CommentService(
                _commentRepositoryMock.Object,
                _userRepositoryMock.Object,
                _newsRepositoryMock.Object,
                _bannedWordRepositoryMock.Object);
        }

        private static User CreateAuthor(int id = 1)
        {
            return new User
            {
                Id = id,
                Username = "subscriber1",
                Name = "Анна",
                Surname = "Читателева",
                ParentName = "Андреевна"
            };
        }

        // ---------- Позитивный сценарий: обычный комментарий создаётся ----------
        [Fact]
        public async Task CreateAsync_NoBannedWords_CreatesComment()
        {
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new News { Id = 1, Title = "Заголовок", Text = "Текст", InsertedBy = 2 });

            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var createdComment = new Comment
            {
                Id = 1,
                Text = "Отличная новость!",
                NewsId = 1,
                CreatedAt = DateTime.UtcNow,
                InsertedBy = 4
            };

            _commentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Comment>()))
                .ReturnsAsync(createdComment);

            _userRepositoryMock.Setup(r => r.GetByIdAsync(4))
                .ReturnsAsync(CreateAuthor(4));

            var request = new CreateCommentRequest { Text = "Отличная новость!" };

            var result = await _service.CreateAsync(1, request, userId: 4);

            Assert.Equal("Отличная новость!", result.Text);
            _userRepositoryMock.Verify(r => r.RegisterFailedBannedWordAttemptAsync(It.IsAny<int>()), Times.Never);
        }

        // ---------- Негативный сценарий: новость не найдена ----------
        [Fact]
        public async Task CreateAsync_NewsNotFound_ThrowsNotFoundException()
        {
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((News?)null);

            var request = new CreateCommentRequest { Text = "Текст" };

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.CreateAsync(99, request, userId: 4));
        }

        // ---------- Негативный сценарий: запрещённое слово в комментарии ----------
        [Fact]
        public async Task CreateAsync_BannedWord_ThrowsBannedWordException()
        {
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new News { Id = 1, Title = "Заголовок", Text = "Текст", InsertedBy = 2 });

            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var request = new CreateCommentRequest { Text = "дурак" };

            await Assert.ThrowsAsync<BannedWordException>(
                () => _service.CreateAsync(1, request, userId: 4));

            _userRepositoryMock.Verify(r => r.RegisterFailedBannedWordAttemptAsync(4), Times.Once);
        }

        // ---------- UpdateAsync: комментарий не найден ----------
        [Fact]
        public async Task UpdateAsync_CommentNotFound_ThrowsNotFoundException()
        {
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Comment?)null);

            var request = new UpdateCommentRequest { Text = "Новый текст" };

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateAsync(99, request, userId: 1, userRole: "subscriber"));
        }

        // ---------- UpdateAsync: нет прав (не автор и не admin) ----------
        [Fact]
        public async Task UpdateAsync_NotOwnerAndNotAdmin_ThrowsUnauthorizedAccessException()
        {
            var comment = new Comment { Id = 1, Text = "Старый текст", NewsId = 1, InsertedBy = 1 };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            var request = new UpdateCommentRequest { Text = "Новый текст" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.UpdateAsync(1, request, userId: 2, userRole: "subscriber"));
        }

        // ---------- UpdateAsync: запрещённое слово ----------
        [Fact]
        public async Task UpdateAsync_BannedWord_ThrowsBannedWordException()
        {
            var comment = new Comment { Id = 1, Text = "Старый текст", NewsId = 1, InsertedBy = 1 };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            var request = new UpdateCommentRequest { Text = "дурак" };

            await Assert.ThrowsAsync<BannedWordException>(
                () => _service.UpdateAsync(1, request, userId: 1, userRole: "subscriber"));
        }

        // ---------- DeleteAsync: комментарий не найден ----------
        [Fact]
        public async Task DeleteAsync_CommentNotFound_ThrowsNotFoundException()
        {
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Comment?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteAsync(99, userId: 1, userRole: "subscriber"));
        }

        // ---------- DeleteAsync: нет прав ----------
        [Fact]
        public async Task DeleteAsync_NotOwnerAndNotAdmin_ThrowsUnauthorizedAccessException()
        {
            var comment = new Comment { Id = 1, Text = "Текст", NewsId = 1, InsertedBy = 1 };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.DeleteAsync(1, userId: 2, userRole: "subscriber"));
        }

        // ---------- DeleteAsync: admin может удалить чужой комментарий ----------
        [Fact]
        public async Task DeleteAsync_Admin_DeletesAnyComment()
        {
            var comment = new Comment { Id = 1, Text = "Текст", NewsId = 1, InsertedBy = 1 };
            _commentRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            await _service.DeleteAsync(1, userId: 999, userRole: "admin");

            _commentRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}