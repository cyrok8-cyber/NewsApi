using System.ComponentModel.DataAnnotations;

namespace NewsApi.Entities
{
    // Сообщение во внутреннем чате редакции.
    // Текст хранится уже после цензуры (цепочка обработчиков в Services/Censorship)
    public class ChatMessage
    {
        public int Id { get; set; }

        public int EditorialOfficeId { get; set; }
        public EditorialOffice EditorialOffice { get; set; } = null!;

        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;

        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        // true - цензура изменила исходный текст сообщения
        public bool WasCensored { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
