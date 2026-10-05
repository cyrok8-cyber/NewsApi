using Microsoft.EntityFrameworkCore;
using NewsApi.Entities;

namespace NewsApi.Data
{
    // Главный класс для работы с базой данных.
    // Через него Entity Framework Core узнаёт, какие таблицы нужно создать
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Каждый DbSet соответствует одной таблице в базе данных
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<News> News { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
}