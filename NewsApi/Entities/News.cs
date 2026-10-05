using System.ComponentModel.DataAnnotations;

namespace NewsApi.Entities
{
    // Новость: основная сущность системы
    public class News
    {
        public int Id { get; set; }

        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }

        // Id пользователя, создавшего новость
        public int InsertedBy { get; set; }

        // Id пользователя, отредактировавшего новость (может быть пустым)
        public int? UpdatedBy { get; set; }
    }
}