using NewsApi.DTOs.Requests;
using NewsApi.DTOs.Responses;
using NewsApi.Entities;
using NewsApi.Exceptions;
using NewsApi.Repositories;

namespace NewsApi.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly INewsRepository _newsRepository; // напрямую репозиторий, не NewsService

        public CommentService(ICommentRepository repository, IUserRepository userRepository, INewsRepository newsRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _newsRepository = newsRepository;
        }

        public async Task<List<CommentResponse>> GetByNewsIdAsync(int newsId)
        {
            var comments = await _repository.GetByNewsIdAsync(newsId);

            var items = new List<CommentResponse>();
            foreach (var comment in comments)
            {
                items.Add(await MapToResponseAsync(comment));
            }
            return items;
        }

        public async Task<CommentResponse> CreateAsync(int newsId, CreateCommentRequest request, int userId)
        {
            // Нельзя оставить комментарий к новости, которой не существует
            var news = await _newsRepository.GetByIdAsync(newsId);
            if (news == null)
                throw new NotFoundException($"Новость с id {newsId} не найдена");

            var comment = new Comment
            {
                Text = request.Text,
                NewsId = newsId,
                CreatedAt = DateTime.UtcNow,
                InsertedBy = userId
            };

            var created = await _repository.CreateAsync(comment);
            return await MapToResponseAsync(created);
        }

        public async Task UpdateAsync(int id, UpdateCommentRequest request, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException($"Комментарий с id {id} не найден");

            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на редактирование этого комментария");

            existing.Text = request.Text;
            await _repository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotFoundException($"Комментарий с id {id} не найден");

            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на удаление этого комментария");

            await _repository.DeleteAsync(id);
        }

        private async Task<CommentResponse> MapToResponseAsync(Comment comment)
        {
            var user = await _userRepository.GetByIdAsync(comment.InsertedBy);

            return new CommentResponse
            {
                Id = comment.Id,
                Text = comment.Text,
                NewsId = comment.NewsId,
                CreatedAt = comment.CreatedAt,
                EditedAt = comment.EditedAt,
                InsertedBy = user != null ? new UserShortResponse
                {
                    Id = user.Id,
                    Surname = user.Surname,
                    Name = user.Name,
                    ParentName = user.ParentName
                } : null!
            };
        }
    }
}