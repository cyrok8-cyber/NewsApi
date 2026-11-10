using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs.Requests
{
    // Данные для изменения редакции (только admin)
    public class UpdateEditorialOfficeRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
