using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;

namespace NewsApi.Services
{
    // Бизнес-логика редакций. Права (только admin) проверяются атрибутами в контроллере,
    // здесь - правила предметной области: уникальность наименования,
    // в редакции состоят только журналисты, журналист - максимум в одной редакции
    public class EditorialOfficeService : IEditorialOfficeService
    {
        private const string ReporterRole = "reporter";

        private readonly IEditorialOfficeRepository _officeRepository;
        private readonly IUserRepository _userRepository;

        public EditorialOfficeService(
            IEditorialOfficeRepository officeRepository,
            IUserRepository userRepository)
        {
            _officeRepository = officeRepository;
            _userRepository = userRepository;
        }

        public async Task<List<EditorialOfficeResponse>> GetAllAsync()
        {
            var offices = await _officeRepository.GetAllAsync();
            return offices.Select(MapToResponse).ToList();
        }

        public async Task<EditorialOfficeResponse> GetByIdAsync(int id)
        {
            var office = await GetOfficeOrThrowAsync(id);
            return MapToResponse(office);
        }

        public async Task<EditorialOfficeResponse> CreateAsync(CreateEditorialOfficeRequest request)
        {
            var name = request.Name.Trim();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Наименование редакции не может быть пустым");

            if (await _officeRepository.GetByNameAsync(name) is not null)
                throw new ConflictException($"Редакция «{name}» уже существует");

            var office = new EditorialOffice
            {
                Name = name,
                Description = NormalizeDescription(request.Description),
                CreatedAt = DateTime.UtcNow
            };

            var created = await _officeRepository.CreateAsync(office);
            return MapToResponse(created);
        }

        public async Task UpdateAsync(int id, UpdateEditorialOfficeRequest request)
        {
            var office = await GetOfficeOrThrowAsync(id);

            var name = request.Name.Trim();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Наименование редакции не может быть пустым");

            var sameName = await _officeRepository.GetByNameAsync(name);
            if (sameName is not null && sameName.Id != id)
                throw new ConflictException($"Редакция «{name}» уже существует");

            office.Name = name;
            office.Description = NormalizeDescription(request.Description);

            await _officeRepository.UpdateAsync(office);
        }

        public async Task DeleteAsync(int id)
        {
            var office = await GetOfficeOrThrowAsync(id);
            await _officeRepository.DeleteAsync(office);
        }

        // Добавить журналиста в редакцию
        public async Task AddMemberAsync(int officeId, int userId)
        {
            var office = await GetOfficeOrThrowAsync(officeId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"Пользователь с id={userId} не найден");

            if (user.Role?.Name != ReporterRole)
                throw new ArgumentException("В редакцию можно добавить только журналиста (роль reporter)");

            if (user.EditorialOfficeId == officeId)
                throw new ConflictException("Пользователь уже состоит в этой редакции");

            if (user.EditorialOfficeId.HasValue)
            {
                var currentName = user.EditorialOffice?.Name ?? $"id={user.EditorialOfficeId}";
                throw new ConflictException(
                    $"Пользователь уже состоит в редакции «{currentName}». Сначала исключите его оттуда");
            }

            user.EditorialOfficeId = office.Id;
            user.EditorialOffice = office;
            await _userRepository.UpdateAsync(user);
        }

        // Исключить журналиста из редакции - он становится одиночкой
        public async Task RemoveMemberAsync(int officeId, int userId)
        {
            await GetOfficeOrThrowAsync(officeId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new NotFoundException($"Пользователь с id={userId} не найден");

            if (user.EditorialOfficeId != officeId)
                throw new NotFoundException("Пользователь не состоит в этой редакции");

            user.EditorialOfficeId = null;
            user.EditorialOffice = null;
            await _userRepository.UpdateAsync(user);
        }

        private async Task<EditorialOffice> GetOfficeOrThrowAsync(int id)
        {
            var office = await _officeRepository.GetByIdAsync(id);
            if (office is null)
                throw new NotFoundException($"Редакция с id={id} не найдена");
            return office;
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        private static EditorialOfficeResponse MapToResponse(EditorialOffice office)
        {
            return new EditorialOfficeResponse
            {
                Id = office.Id,
                Name = office.Name,
                Description = office.Description,
                CreatedAt = office.CreatedAt,
                Members = office.Members
                    .OrderBy(u => u.Surname)
                    .Select(u => new UserShortResponse
                    {
                        Id = u.Id,
                        Surname = u.Surname,
                        Name = u.Name,
                        ParentName = u.ParentName
                    })
                    .ToList()
            };
        }
    }
}
