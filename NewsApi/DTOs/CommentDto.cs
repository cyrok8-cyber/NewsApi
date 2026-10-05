using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int NewsId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public UserShortDto InsertedBy { get; set; } = null!;
    }

    // Для создания комментария - newsId придёт прямо в URL, тут его не дублируем
    public class CreateCommentDto
    {
        [Required, MaxLength(300)]
        public string Text { get; set; } = string.Empty;
    }

    public class UpdateCommentDto
    {
        [Required, MaxLength(300)]
        public string Text { get; set; } = string.Empty;
    }
}