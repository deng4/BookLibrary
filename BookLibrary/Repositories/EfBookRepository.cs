using BookLibrary.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Repositories
{
    public class EfBookRepository : IBookRepository
    {
        private readonly LibraryDbContext _context;

        public EfBookRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            // Include(b => b.Authors) загружает связанных авторов
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.CurrentReader)
                //.AsNoTracking()
                .ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(Guid id)
        {
            return await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.CurrentReader)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Book book)
        {
            // Важно: Авторы должны уже существовать в БД.
            // При создании книги мы не создаем новых авторов.
            // Их нужно выбрать из существующих.
            // Логика добавления связей будет в контроллере.

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            // Attach book to context and mark it as modified
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Book>> GetBooksByReaderIdAsync(Guid readerId)
        {
            return await _context.Books
                .Where(b => b.CurrentReaderId == readerId)
                .Include(b => b.Authors)
                //.AsNoTracking()
                .ToListAsync();
        }
    }
}
