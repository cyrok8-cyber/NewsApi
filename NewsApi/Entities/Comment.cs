using System.ComponentModel.DataAnnotations;

namespace NewsApi.Entities
{
    // Комментарий, всегда привязан к конкретной новости
    public class Comment
    {
        public int Id { get; set; }

        [MaxLength(300)]
        public string Text { get; set; } = string.Empty;

        // Id новости, к которой относится комментарий
        public int NewsId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }

        // Id пользователя, создавшего комментарий
        public int InsertedBy { get; set; }
    }
}