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
        public DbSet<EditorialOffice> EditorialOffices { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

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

            // Редакции: наименование уникально
            modelBuilder.Entity<EditorialOffice>().HasIndex(o => o.Name).IsUnique();

            // Журналист -> редакция (необязательная связь).
            // При удалении редакции её журналисты становятся одиночками
            modelBuilder.Entity<User>()
                .HasOne(u => u.EditorialOffice)
                .WithMany(o => o.Members)
                .HasForeignKey(u => u.EditorialOfficeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Новость -> редакция, от имени которой она опубликована (необязательная связь).
            // При удалении редакции новости остаются, но подписываются только именем автора
            modelBuilder.Entity<News>()
                .HasOne(n => n.EditorialOffice)
                .WithMany()
                .HasForeignKey(n => n.EditorialOfficeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Чат редакции: при удалении редакции удаляется и её переписка
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.EditorialOffice)
                .WithMany()
                .HasForeignKey(m => m.EditorialOfficeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Author)
                .WithMany()
                .HasForeignKey(m => m.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Сообщения всегда выбираются по редакции и сортируются по времени
            modelBuilder.Entity<ChatMessage>()
                .HasIndex(m => new { m.EditorialOfficeId, m.CreatedAt });
        }
    }
}
