using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs.Requests
{
    // Данные, которые пользователь передаёт при создании новости
    public class CreateNewsRequest
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Text { get; set; } = string.Empty;
    }
}