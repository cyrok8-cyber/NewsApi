using System.ComponentModel.DataAnnotations;

namespace NewsApi.Entities
{
    // Пользователь системы: администратор, журналист или подписчик
    public class User
    {
        public int Id { get; set; }

        [MaxLength(40)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(80)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Surname { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ParentName { get; set; } = string.Empty; // отчество

        // Роль нужна отдельно для проверки прав доступа (admin/reporter/subscriber)
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}