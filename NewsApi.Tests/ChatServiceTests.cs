using Moq;
using NewsApi.DTOs.Requests;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services;
using NewsApi.Services.Censorship;
using Xunit;

namespace NewsApi.Tests
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatMessageRepository> _chatRepositoryMock = new();
        private readonly Mock<IEditorialOfficeRepository> _officeRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly ChatService _service;

        public ChatServiceTests()
        {
            var bannedWordRepositoryMock = new Mock<IBannedWordRepository>();
            bannedWordRepositoryMock.Setup(r => r.GetAllWordsAsync())
                .ReturnsAsync(new List<string> { "дурак" });

            // Настоящая цепочка цензуры - проверяем, что сервис действительно через неё пропускает текст
            var censorship = new ChatCensorshipChain(
                new EmptyMessageHandler(),
                new MaxLengthHandler(),
                new LinkFilterHandler(),
                new BannedWordsHandler(bannedWordRepositoryMock.Object),
                new CapsLockHandler());

            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new EditorialOffice { Id = 1, Name = "Городские вести" });

            _chatRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<ChatMessage>()))
                .ReturnsAsync((ChatMessage m) => { m.Id = 100; return m; });

            _service = new ChatService(
                _chatRepositoryMock.Object,
                _officeRepositoryMock.Object,
                _userRepositoryMock.Object,
                censorship);
        }

        private void SetupUser(int id, string role, int? officeId)
        {
            _userRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(new User
            {
                Id = id,
                Name = "Пётр",
                Surname = "Журналистов",
                ParentName = "Петрович",
                Role = new Role { Name = role },
                EditorialOfficeId = officeId
            });
        }

        [Fact]
        public async Task SendMessageAsync_Member_SavesCensoredText()
        {
            SetupUser(2, "reporter", officeId: 1);

            var result = await _service.SendMessageAsync(1, 2, "reporter",
                new SendChatMessageRequest { Text = "Какой-то дурак сломал вёрстку" });

            Assert.Equal("Какой-то ***** сломал вёрстку", result.Text);
            Assert.True(result.WasCensored);
            _chatRepositoryMock.Verify(r => r.CreateAsync(It.Is<ChatMessage>(m =>
                m.EditorialOfficeId == 1 && m.AuthorId == 2 && m.WasCensored)), Times.Once);
        }

        [Fact]
        public async Task SendMessageAsync_ReporterFromAnotherOffice_ThrowsForbidden()
        {
            SetupUser(3, "reporter", officeId: null); // журналист-одиночка

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.SendMessageAsync(1, 3, "reporter", new SendChatMessageRequest { Text = "Привет" }));

            _chatRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<ChatMessage>()), Times.Never);
        }

        [Fact]
        public async Task SendMessageAsync_EmptyText_ThrowsArgumentException()
        {
            SetupUser(2, "reporter", officeId: 1);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.SendMessageAsync(1, 2, "reporter", new SendChatMessageRequest { Text = "   " }));
        }

        [Fact]
        public async Task SendMessageAsync_OfficeNotFound_ThrowsNotFound()
        {
            _officeRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((EditorialOffice?)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.SendMessageAsync(99, 2, "reporter", new SendChatMessageRequest { Text = "Привет" }));
        }

        [Fact]
        public async Task GetMessagesAsync_Admin_CanReadAnyOffice()
        {
            _chatRepositoryMock.Setup(r => r.GetByOfficeAsync(1, 1, 50)).ReturnsAsync(new List<ChatMessage>
            {
                new() { Id = 2, Text = "Второе", AuthorId = 5, Author = new User { Id = 5 }, CreatedAt = DateTime.UtcNow },
                new() { Id = 1, Text = "Первое", AuthorId = 2, Author = new User { Id = 2 }, CreatedAt = DateTime.UtcNow.AddMinutes(-5) }
            });
            _chatRepositoryMock.Setup(r => r.CountByOfficeAsync(1)).ReturnsAsync(2);

            var result = await _service.GetMessagesAsync(1, userId: 1, role: "admin", page: 1, pageSize: 50);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal("Первое", result.Items[0].Text); // внутри страницы - от старых к новым
            _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetMessagesAsync_NotMember_ThrowsForbidden()
        {
            SetupUser(3, "reporter", officeId: 2);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.GetMessagesAsync(1, 3, "reporter", 1, 50));
        }
    }
}
