using System.ComponentModel.DataAnnotations;

namespace NewsApi.DTOs
{
    // Что отдаём наружу о пользователе. Пароля здесь нет намеренно
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
    }

    // Что принимаем при регистрации: ФИО, роль, username и password (как в задании).
    // Атрибуты проверяют ограничения по длине автоматически, при нарушении вернётся 400
    public class RegisterUserDto
    {
        [Required, MaxLength(40)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(4), MaxLength(80)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Surname { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ParentName { get; set; }

        [Required, RegularExpression("^(admin|reporter|subscriber)$",
            ErrorMessage = "Роль должна быть одной из: admin, reporter, subscriber")]
        public string Role { get; set; } = string.Empty;
    }
}