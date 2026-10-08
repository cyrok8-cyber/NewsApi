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