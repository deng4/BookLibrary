using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Models.DatabaseModels
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Reader> Readers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация связи "многие-ко-многим" между Book и Author
            // EF Core 5+ может делать это автоматически, но явная конфигурация надежнее.
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books)
                .UsingEntity(j => j.ToTable("BookAuthors")); // Указываем имя связующей таблицы

            // Конфигурация связи "один-ко-многим" между Reader и Book
            modelBuilder.Entity<Reader>()
                .HasMany(r => r.BorrowedBooks)
                .WithOne(b => b.CurrentReader)
                .HasForeignKey(b => b.CurrentReaderId)
                .OnDelete(DeleteBehavior.SetNull); // При удалении читателя, у книг сбрасывается CurrentReaderId в null

            // Уникальный индекс для Email читателя
            modelBuilder.Entity<Reader>()
                .HasIndex(r => r.Email)
                .IsUnique();
        }
    }
}
