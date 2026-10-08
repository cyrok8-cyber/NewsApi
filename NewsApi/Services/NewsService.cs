using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;
using NewsApi.Entities;
using NewsApi.Exceptions;
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

        public async Task<PagedResponse<NewsResponse>> GetAllAsync(int page, int pageSize)
        {
            // Нормализация параметров пагинации - бизнес-правило, поэтому живёт здесь, а не в контроллере
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var news = await _repository.GetAllAsync(page, pageSize);
            var totalCount = await _repository.GetTotalCountAsync();

            var items = new List<NewsResponse>();
            foreach (var item in news)
            {
                items.Add(await MapToResponseAsync(item));
            }

            return new PagedResponse<NewsResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<NewsResponse> GetByIdAsync(int id)
        {
            var news = await _repository.GetByIdAsync(id);
            if (news == null)
                throw new NotFoundException($"Новость с id {id} не найдена");

            return await MapToResponseAsync(news);
        }

        public async Task<NewsResponse> CreateAsync(CreateNewsRequest request, int userId)
        {
            var news = new News
            {
                Title = request.Title,
                Text = request.Text,
                CreatedAt = DateTime.UtcNow,
                InsertedBy = userId
            };

            var created = await _repository.CreateAsync(news);
            return await MapToResponseAsync(created);
        }

        public async Task UpdateAsync(int id, UpdateNewsRequest request, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException($"Новость с id {id} не найдена");

            // admin может всё, reporter - только свои новости
            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на редактирование этой новости");

            existing.Title = request.Title;
            existing.Text = request.Text;
            existing.UpdatedBy = userId;

            await _repository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException($"Новость с id {id} не найдена");

            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на удаление этой новости");

            await _repository.DeleteAsync(id);
        }

        private async Task<NewsResponse> MapToResponseAsync(News news)
        {
            var insertedByUser = await _userRepository.GetByIdAsync(news.InsertedBy);

            UserShortResponse? updatedByUser = null;
            if (news.UpdatedBy.HasValue)
            {
                var updatedUser = await _userRepository.GetByIdAsync(news.UpdatedBy.Value);
                if (updatedUser != null)
                    updatedByUser = MapToUserShort(updatedUser);
            }

            return new NewsResponse
            {
                Id = news.Id,
                Title = news.Title,
                Text = news.Text,
                CreatedAt = news.CreatedAt,
                EditedAt = news.EditedAt,
                InsertedBy = insertedByUser != null ? MapToUserShort(insertedByUser) : null!,
                UpdatedBy = updatedByUser
            };
        }

        private static UserShortResponse MapToUserShort(Entities.User user)
        {
            return new UserShortResponse
            {
                Id = user.Id,
                Surname = user.Surname,
                Name = user.Name,
                ParentName = user.ParentName
            };
        }
    }
}