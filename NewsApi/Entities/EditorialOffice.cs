using System.ComponentModel.DataAnnotations;

namespace NewsApi.Entities
{
    // Редакция - объединение журналистов (роль reporter), которые публикуют новости от её имени.
    // Создавать редакции и управлять их составом может только администратор.
    // Журналист может состоять максимум в одной редакции или работать в одиночку (без редакции).
    public class EditorialOffice
    {
        public int Id { get; set; }

        // Наименование редакции - выводится в новости перед именем редактора
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        // Журналисты, которые состоят в редакции
        public List<User> Members { get; set; } = new();
    }
}
