using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs.Requests
{
    public class BlockUserRequest
    {
        [Required, Range(1, int.MaxValue, ErrorMessage = "Длительность блокировки должна быть больше 0 минут")]
        public int DurationMinutes { get; set; }
    }
}