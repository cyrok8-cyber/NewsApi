using Microsoft.EntityFrameworkCore;
using NewsApi.Entities;

namespace NewsApi.Data
{
    // Базовый контекст - описывает таблицы, не привязан к конкретному провайдеру базы
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<News> News { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
}