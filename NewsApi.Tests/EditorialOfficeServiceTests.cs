using Moq;
using NewsApi.DTOs.Requests;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;
using NewsApi.Services;
using Xunit;

namespace NewsApi.Tests
{
    public class EditorialOfficeServiceTests
    {
        private readonly Mock<IEditorialOfficeRepository> _officeRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly EditorialOfficeService _service;

        public EditorialOfficeServiceTests()
        {
            _officeRepositoryMock = new Mock<IEditorialOfficeRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _service = new EditorialOfficeService(
                _officeRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        private static EditorialOffice CreateOffice(int id = 1, string name = "Городские вести")
        {
            return new EditorialOffice { Id = id, Name = name, CreatedAt = DateTime.UtcNow };
        }

        private static User CreateUser(int id, string role, int? officeId = null, EditorialOffice? office = null)
        {
            return new User
            {
                Id = id,
                Username = $"user{id}",
                Name = "Пётр",
                Surname = "Журналистов",
                ParentName = "Петрович",
                Role = new Role { Name = role },
                EditorialOfficeId = officeId,
                EditorialOffice = office
            };
        }

        // ---------- Создание редакции ----------
        [Fact]
        public async Task CreateAsync_UniqueName_CreatesOffice()
        {
            _officeRepositoryMock.Setup(r => r.GetByNameAsync("Городские вести"))
                .ReturnsAsync((EditorialOffice?)null);
            _officeRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<EditorialOffice>()))
                .ReturnsAsync((EditorialOffice o) => { o.Id = 10; return o; });

            var result = await _service.CreateAsync(new CreateEditorialOfficeRequest
            {
                Name = "  Городские вести  ",
                Description = "Новости города"
            });

            Assert.Equal(10, result.Id);
            Assert.Equal("Городские вести", result.Name); // пробелы по краям обрезаются
            Assert.Empty(result.Members);
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ThrowsConflictException()
        {
            _officeRepositoryMock.Setup(r => r.GetByNameAsync("Городские вести"))
                .ReturnsAsync(CreateOffice());

            await Assert.ThrowsAsync<ConflictException>(() =>
                _service.CreateAsync(new CreateEditorialOfficeRequest { Name = "Городские вести" }));

            _officeRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<EditorialOffice>()), Times.Never);
        }

        // ---------- Изменение и удаление ----------
        [Fact]
        public async Task UpdateAsync_NameTakenByAnotherOffice_ThrowsConflictException()
        {
            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateOffice(1, "Городские вести"));
            _officeRepositoryMock.Setup(r => r.GetByNameAsync("Спортивный вестник"))
                .ReturnsAsync(CreateOffice(2, "Спортивный вестник"));

            await Assert.ThrowsAsync<ConflictException>(() =>
                _service.UpdateAsync(1, new UpdateEditorialOfficeRequest { Name = "Спортивный вестник" }));
        }

        [Fact]
        public async Task DeleteAsync_OfficeNotFound_ThrowsNotFoundException()
        {
            _officeRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((EditorialOffice?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(99));
        }

        // ---------- Добавление журналиста ----------
        [Fact]
        public async Task AddMemberAsync_SingleReporter_JoinsOffice()
        {
            var office = CreateOffice();
            var reporter = CreateUser(2, "reporter");

            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(office);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(reporter);

            await _service.AddMemberAsync(1, 2);

            Assert.Equal(1, reporter.EditorialOfficeId);
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.EditorialOfficeId == 1)), Times.Once);
        }

        [Fact]
        public async Task AddMemberAsync_NotReporter_ThrowsArgumentException()
        {
            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateOffice());
            _userRepositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(CreateUser(4, "subscriber"));

            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddMemberAsync(1, 4));
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task AddMemberAsync_ReporterInAnotherOffice_ThrowsConflictException()
        {
            var otherOffice = CreateOffice(2, "Спортивный вестник");
            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateOffice());
            _userRepositoryMock.Setup(r => r.GetByIdAsync(2))
                .ReturnsAsync(CreateUser(2, "reporter", officeId: 2, office: otherOffice));

            var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.AddMemberAsync(1, 2));
            Assert.Contains("Спортивный вестник", ex.Message);
        }

        [Fact]
        public async Task AddMemberAsync_UserNotFound_ThrowsNotFoundException()
        {
            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateOffice());
            _userRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.AddMemberAsync(1, 99));
        }

        // ---------- Исключение журналиста ----------
        [Fact]
        public async Task RemoveMemberAsync_Member_BecomesSingle()
        {
            var office = CreateOffice();
            var reporter = CreateUser(2, "reporter", officeId: 1, office: office);

            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(office);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(reporter);

            await _service.RemoveMemberAsync(1, 2);

            Assert.Null(reporter.EditorialOfficeId);
            Assert.Null(reporter.EditorialOffice);
            _userRepositoryMock.Verify(r => r.UpdateAsync(reporter), Times.Once);
        }

        [Fact]
        public async Task RemoveMemberAsync_NotMember_ThrowsNotFoundException()
        {
            _officeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateOffice());
            _userRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(CreateUser(3, "reporter"));

            await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoveMemberAsync(1, 3));
        }
    }
}
