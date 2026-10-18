using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs.Requests
{
    public class RegisterUserRequest
    {
        [Required, MaxLength(40)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(4), MaxLength(80)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        [RegularExpression(@"^[А-Яа-яЁё]+(-[А-Яа-яЁё]+)*$",
            ErrorMessage = "Имя может содержать только буквы и дефис (не в начале/конце и не подряд)")]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        [RegularExpression(@"^[А-Яа-яЁё]+(-[А-Яа-яЁё]+)*$",
            ErrorMessage = "Фамилия может содержать только буквы и дефис (не в начале/конце и не подряд)")]
        public string Surname { get; set; } = string.Empty;

        [MaxLength(20)]
        [RegularExpression(@"^[А-Яа-яЁё]+(-[А-Яа-яЁё]+)*$",
            ErrorMessage = "Отчество может содержать только буквы и дефис (не в начале/конце и не подряд)")]
        public string? ParentName { get; set; }

        [Required, RegularExpression("^(admin|reporter|subscriber)$",
            ErrorMessage = "Роль должна быть одной из: admin, reporter, subscriber")]
        public string Role { get; set; } = string.Empty;
    }
}