using Microsoft.EntityFrameworkCore;

namespace NewsApi.Data
{
    // Отдельный класс нужен, чтобы EF Core мог различать миграции SQLite и PostgreSQL
    public class SqliteAppDbContext : AppDbContext
    {
        public SqliteAppDbContext(DbContextOptions<SqliteAppDbContext> options) : base(options)
        {
        }
    }
}