using NewsApi.DTOs;
using NewsApi.Entities;
using NewsApi.Repositories;

namespace NewsApi.Services
{
    public class NewsService : INewsService
    {
        private readonly INewsRepository _repository;
        private readonly IUserRepository _userRepository;

        public NewsService(INewsRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<PagedResult<NewsDto>> GetAllAsync(int page, int pageSize)
        {
            var news = await _repository.GetAllAsync(page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            var items = new List<NewsDto>();
            foreach (var item in news)
            {
                items.Add(await MapToDtoAsync(item));
            }

            return new PagedResult<NewsDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<NewsDto?> GetByIdAsync(int id)
        {
            var news = await _repository.GetByIdAsync(id);
            return news == null ? null : await MapToDtoAsync(news);
        }

        public async Task<NewsDto> CreateAsync(CreateNewsDto dto, int userId)
        {
            var news = new News
            {
                Title = dto.Title,
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow,
                InsertedBy = userId
            };

            var created = await _repository.CreateAsync(news);
            return await MapToDtoAsync(created);
        }

        public async Task<bool> UpdateAsync(int id, UpdateNewsDto dto, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            // Проверка прав: admin может всё, reporter - только свои новости
            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на редактирование этой новости");

            existing.Title = dto.Title;
            existing.Text = dto.Text;
            existing.UpdatedBy = userId;

            return await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на удаление этой новости");

            return await _repository.DeleteAsync(id);
        }

        // Превращаем сущность News в NewsDto, подставляя короткую информацию об авторах
        private async Task<NewsDto> MapToDtoAsync(News news)
        {
            var insertedByUser = await _userRepository.GetByIdAsync(news.InsertedBy);

            UserShortDto? updatedByUser = null;
            if (news.UpdatedBy.HasValue)
            {
                var updatedUser = await _userRepository.GetByIdAsync(news.UpdatedBy.Value);
                if (updatedUser != null)
                    updatedByUser = MapToUserShortDto(updatedUser);
            }

            return new NewsDto
            {
                Id = news.Id,
                Title = news.Title,
                Text = news.Text,
                CreatedAt = news.CreatedAt,
                EditedAt = news.EditedAt,
                InsertedBy = insertedByUser != null ? MapToUserShortDto(insertedByUser) : null!,
                UpdatedBy = updatedByUser
            };
        }

        private static UserShortDto MapToUserShortDto(Entities.User user)
        {
            return new UserShortDto
            {
                Id = user.Id,
                Surname = user.Surname,
                Name = user.Name,
                ParentName = user.ParentName
            };
        }
    }
}