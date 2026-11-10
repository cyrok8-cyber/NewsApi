using Moq;
using NewsApi.DTOs.Requests;
using NewsApi.Entities;
using NewsApi.Repositories;
using NewsApi.Services;
using Xunit;

namespace NewsApi.Tests
{
    // Проверяем, что новость публикуется от имени редакции автора
    // и что в подписи сначала идёт редакция, а потом редактор
    public class NewsPublishedByTests
    {
        private readonly Mock<INewsRepository> _newsRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IBannedWordRepository> _bannedWordRepositoryMock = new();
        private readonly NewsService _service;

        public NewsPublishedByTests()
        {
            _bannedWordRepositoryMock.Setup(r => r.ContainsBannedWordAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _service = new NewsService(
                _newsRepositoryMock.Object,
                _userRepositoryMock.Object,
                _bannedWordRepositoryMock.Object);
        }

        private static User CreateReporter(EditorialOffice? office)
        {
            return new User
            {
                Id = 2,
                Username = "reporter1",
                Name = "Пётр",
                Surname = "Журналистов",
                ParentName = "Петрович",
                Role = new Role { Name = "reporter" },
                EditorialOfficeId = office?.Id,
                EditorialOffice = office
            };
        }

        // Репозиторий "сохраняет" новость: выдаём id и подставляем редакцию, как это делает EF
        private void SetupCreate(EditorialOffice? office)
        {
            _newsRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<News>()))
                .ReturnsAsync((News n) =>
                {
                    n.Id = 1;
                    n.EditorialOffice = office;
                    return n;
                });
        }

        private static CreateNewsRequest Request() => new()
        {
            Title = "Открылся новый парк",
            Text = "Сегодня состоялось открытие парка"
        };

        [Fact]
        public async Task CreateAsync_ReporterInOffice_PublishesOnBehalfOfOffice()
        {
            var office = new EditorialOffice { Id = 1, Name = "Городские вести" };
            _userRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(CreateReporter(office));
            SetupCreate(office);

            var result = await _service.CreateAsync(Request(), userId: 2);

            _newsRepositoryMock.Verify(r => r.CreateAsync(It.Is<News>(n => n.EditorialOfficeId == 1)), Times.Once);
            Assert.NotNull(result.EditorialOffice);
            Assert.Equal("Городские вести", result.EditorialOffice!.Name);
            Assert.Equal("Редакция «Городские вести», редактор: Журналистов Пётр Петрович", result.PublishedBy);
        }

        [Fact]
        public async Task CreateAsync_SingleReporter_PublishesOnOwnBehalf()
        {
            _userRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(CreateReporter(null));
            SetupCreate(null);

            var result = await _service.CreateAsync(Request(), userId: 2);

            _newsRepositoryMock.Verify(r => r.CreateAsync(It.Is<News>(n => n.EditorialOfficeId == null)), Times.Once);
            Assert.Null(result.EditorialOffice);
            Assert.Equal("Журналистов Пётр Петрович", result.PublishedBy);
        }
    }
}
