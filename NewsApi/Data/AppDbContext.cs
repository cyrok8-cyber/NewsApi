using Microsoft.EntityFrameworkCore;
using NewsApi.Entities;

namespace NewsApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<News> News { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<BannedWord> BannedWords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "admin" },
                new Role { Id = 2, Name = "reporter" },
                new Role { Id = 3, Name = "subscriber" }
            );

            
        }
    }
}