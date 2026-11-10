using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs.Requests
{
    // Сообщение в чат редакции. Длину и содержимое проверяет цепочка цензуры
    public class SendChatMessageRequest
    {
        [Required]
        public string Text { get; set; } = string.Empty;
    }
}
