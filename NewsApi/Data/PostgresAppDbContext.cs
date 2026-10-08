using Microsoft.EntityFrameworkCore;

namespace NewsApi.Data
{
    public class PostgresAppDbContext : AppDbContext
    {
        public PostgresAppDbContext(DbContextOptions<PostgresAppDbContext> options) : base(options)
        {
        }
    }
}