namespace NewsApi.DTOs.Responses
{
    public class ChatMessageResponse
    {
        public int Id { get; set; }

        // Текст после цензуры
        public string Text { get; set; } = string.Empty;

        public UserShortResponse Author { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // true - цензура изменила сообщение (ссылки, запрещённые слова, капс)
        public bool WasCensored { get; set; }
    }
}
