using NewsApi.DTOs;
using NewsApi.Entities;
using NewsApi.Repositories;

namespace NewsApi.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _repository;
        private readonly IUserRepository _userRepository;

        public CommentService(ICommentRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<List<CommentDto>> GetByNewsIdAsync(int newsId)
        {
            var comments = await _repository.GetByNewsIdAsync(newsId);

            var items = new List<CommentDto>();
            foreach (var comment in comments)
            {
                items.Add(await MapToDtoAsync(comment));
            }
            return items;
        }

        public async Task<CommentDto> CreateAsync(int newsId, CreateCommentDto dto, int userId)
        {
            var comment = new Comment
            {
                Text = dto.Text,
                NewsId = newsId,
                CreatedAt = DateTime.UtcNow,
                InsertedBy = userId
            };

            var created = await _repository.CreateAsync(comment);
            return await MapToDtoAsync(created);
        }

        public async Task<bool> UpdateAsync(int id, UpdateCommentDto dto, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            // Только admin или сам автор комментария (subscriber) может редактировать
            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на редактирование этого комментария");

            existing.Text = dto.Text;
            return await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id, int userId, string userRole)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            if (userRole != "admin" && existing.InsertedBy != userId)
                throw new UnauthorizedAccessException("Нет прав на удаление этого комментария");

            return await _repository.DeleteAsync(id);
        }

        private async Task<CommentDto> MapToDtoAsync(Comment comment)
        {
            var user = await _userRepository.GetByIdAsync(comment.InsertedBy);

            return new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                NewsId = comment.NewsId,
                CreatedAt = comment.CreatedAt,
                EditedAt = comment.EditedAt,
                InsertedBy = user != null ? new UserShortDto
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