using Moq;
using NewsApi.DTOs.Requests;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services;
using Xunit;

namespace NewsApi.Tests
{
    public class NewsServiceTests
    {
        private readonly Mock<INewsRepository> _newsRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IBannedWordRepository> _bannedWordRepositoryMock;
        private readonly NewsService _service;

        public NewsServiceTests()
        {
            _newsRepositoryMock = new Mock<INewsRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _bannedWordRepositoryMock = new Mock<IBannedWordRepository>();
            _service = new NewsService(
                _newsRepositoryMock.Object,
                _userRepositoryMock.Object,
                _bannedWordRepositoryMock.Object);
        }

        private static User CreateAuthor(int id = 1)
        {
            return new User
            {
                Id = id,
                Username = "reporter1",
                Name = "Пётр",
                Surname = "Журналистов",
                ParentName = "Петрович"
            };
        }

        // ---------- Позитивный сценарий: текст без запрещённых слов ----------
        [Fact]
        public async Task CreateAsync_NoBannedWords_CreatesNews()
        {
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var createdNews = new News
            {
                Id = 1,
                Title = "Обычный заголовок",
                Text = "Обычный текст новости",
                CreatedAt = DateTime.UtcNow,
                InsertedBy = 1
            };

            _newsRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<News>()))
                .ReturnsAsync(createdNews);

            _userRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(CreateAuthor());

            var request = new CreateNewsRequest
            {
                Title = "Обычный заголовок",
                Text = "Обычный текст новости"
            };

            var result = await _service.CreateAsync(request, userId: 1);

            Assert.Equal("Обычный заголовок", result.Title);
            _userRepositoryMock.Verify(r => r.RegisterFailedBannedWordAttemptAsync(It.IsAny<int>()), Times.Never);
        }

        // ---------- Негативный сценарий: запрещённое слово в заголовке ----------
        [Fact]
        public async Task CreateAsync_BannedWordInTitle_ThrowsBannedWordException()
        {
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync("Плохой заголовок"))
                .ReturnsAsync(true);
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync((string text) => text == "Плохой заголовок");

            var request = new CreateNewsRequest
            {
                Title = "Плохой заголовок",
                Text = "Нормальный текст"
            };

            await Assert.ThrowsAsync<BannedWordException>(
                () => _service.CreateAsync(request, userId: 1));
        }

        // ---------- Негативный сценарий: запрещённое слово в тексте ----------
        [Fact]
        public async Task CreateAsync_BannedWordInText_ThrowsBannedWordException()
        {
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync((string text) => text.Contains("дурак"));

            var request = new CreateNewsRequest
            {
                Title = "Нормальный заголовок",
                Text = "Ты дурак"
            };

            await Assert.ThrowsAsync<BannedWordException>(
                () => _service.CreateAsync(request, userId: 1));
        }

        // ---------- Проверка: при запрещённом слове регистрируется неудачная попытка ----------
        [Fact]
        public async Task CreateAsync_BannedWord_RegistersFailedAttempt()
        {
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var request = new CreateNewsRequest
            {
                Title = "дурак",
                Text = "текст"
            };

            await Assert.ThrowsAsync<BannedWordException>(
                () => _service.CreateAsync(request, userId: 5));

            _userRepositoryMock.Verify(r => r.RegisterFailedBannedWordAttemptAsync(5), Times.Once);
        }

        // ---------- UpdateAsync: новость не найдена ----------
        [Fact]
        public async Task UpdateAsync_NewsNotFound_ThrowsNotFoundException()
        {
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((News?)null);

            var request = new UpdateNewsRequest { Title = "Заголовок", Text = "Текст" };

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateAsync(99, request, userId: 1, userRole: "reporter"));
        }

        // ---------- UpdateAsync: нет прав (не автор и не admin) ----------
        [Fact]
        public async Task UpdateAsync_NotOwnerAndNotAdmin_ThrowsUnauthorizedAccessException()
        {
            var news = new News { Id = 1, Title = "Старое", Text = "Старый текст", InsertedBy = 1 };
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(news);

            var request = new UpdateNewsRequest { Title = "Новое", Text = "Новый текст" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.UpdateAsync(1, request, userId: 2, userRole: "reporter"));
        }

        // ---------- UpdateAsync: запрещённое слово ----------
        [Fact]
        public async Task UpdateAsync_BannedWord_ThrowsBannedWordException()
        {
            var news = new News { Id = 1, Title = "Старое", Text = "Старый текст", InsertedBy = 1 };
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(news);
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var request = new UpdateNewsRequest { Title = "дурак", Text = "текст" };

            await Assert.ThrowsAsync<BannedWordException>(
                () => _service.UpdateAsync(1, request, userId: 1, userRole: "reporter"));
        }

        // ---------- DeleteAsync: новость не найдена ----------
        [Fact]
        public async Task DeleteAsync_NewsNotFound_ThrowsNotFoundException()
        {
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((News?)null);

            await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteAsync(99, userId: 1, userRole: "reporter"));
        }

        // ---------- DeleteAsync: нет прав ----------
        [Fact]
        public async Task DeleteAsync_NotOwnerAndNotAdmin_ThrowsUnauthorizedAccessException()
        {
            var news = new News { Id = 1, Title = "Заголовок", Text = "Текст", InsertedBy = 1 };
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(news);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.DeleteAsync(1, userId: 2, userRole: "reporter"));
        }

        // ---------- DeleteAsync: admin может удалить чужую новость ----------
        [Fact]
        public async Task DeleteAsync_Admin_DeletesAnyNews()
        {
            var news = new News { Id = 1, Title = "Заголовок", Text = "Текст", InsertedBy = 1 };
            _newsRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(news);

            await _service.DeleteAsync(1, userId: 999, userRole: "admin");

            _newsRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }
    }
}