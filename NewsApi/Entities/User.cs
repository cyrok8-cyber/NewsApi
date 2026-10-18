using System.ComponentModel.DataAnnotations;

namespace NewsApi.Entities
{
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
        public string ParentName { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }

        // Блокировка
        public int FailedLoginAttempts { get; set; } = 0;
        public int FailedBannedWordAttempts { get; set; } = 0;
        public DateTime? BlockedUntil { get; set; }
    }
}