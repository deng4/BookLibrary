using BookLibrary.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Repositories
{
    public class EfReaderRepository : IReaderRepository
    {
        private readonly LibraryDbContext _context;

        public EfReaderRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reader>> GetAllAsync()
        {
            return await _context.Readers/*.AsNoTracking()*/.ToListAsync();
        }

        public async Task<Reader?> GetByIdAsync(Guid id)
        {
            // Используем Include для подгрузки связанных данных (взятых книг).
            // ThenInclude позволяет загрузить данные из связанной сущности (авторов для взятых книг).
            // Это помогает избежать проблемы N+1 запросов.
            return await _context.Readers
                .Include(r => r.BorrowedBooks)
                    .ThenInclude(b => b.Authors)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(Reader reader)
        {
            _context.Readers.Add(reader);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reader reader)
        {
            _context.Readers.Update(reader);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            // Логика проверки на наличие невозвращенных книг должна быть в контроллере.
            // Хотя мы настроили OnDelete(DeleteBehavior.SetNull) в DbContext,
            // лучше предотвращать удаление на уровне бизнес-логики, а не полагаться только на БД.
            var reader = await _context.Readers.FindAsync(id);
            if (reader != null)
            {
                _context.Readers.Remove(reader);
                await _context.SaveChangesAsync();
            }
        }
    }
}
