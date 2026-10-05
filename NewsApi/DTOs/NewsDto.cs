using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs
{
    // То, что мы отдаём наружу вместо полной сущности News
    
    public class NewsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public UserShortDto InsertedBy { get; set; } = null!;
        public UserShortDto? UpdatedBy { get; set; }
    }


    // DTO для создания новости - принимаем только то, что задаёт пользователь
    public class CreateNewsDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Text { get; set; } = string.Empty;
    }

    // DTO для обновления новости
    public class UpdateNewsDto
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Text { get; set; } = string.Empty;
    }
    // Новость вместе со списком её комментариев
    public class NewsWithCommentsDto
    {
        public NewsDto News { get; set; } = null!;
        public List<CommentDto> Comments { get; set; } = new();
    }
}