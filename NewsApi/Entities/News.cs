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

        // Редакция, от имени которой опубликована новость.
        // Запоминается в момент публикации: если журналист потом уйдёт из редакции,
        // уже опубликованные новости останутся за редакцией.
        // null - новость опубликована журналистом-одиночкой или администратором
        public int? EditorialOfficeId { get; set; }
        public EditorialOffice? EditorialOffice { get; set; }
    }
}