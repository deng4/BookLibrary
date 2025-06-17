using BookLibrary.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace BookLibrary.Repositories
{
    public class EfAuthorRepository : IAuthorRepository
    {
        private readonly LibraryDbContext _context;

        public EfAuthorRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            // AsNoTracking используется для запросов "только для чтения", что повышает производительность.
            return await _context.Authors/*.AsNoTracking()*/.ToListAsync();
        }

        public async Task<Author?> GetByIdAsync(Guid id)
        {
            // FindAsync - это оптимизированный способ получения сущности по первичному ключу.
            return await _context.Authors.FindAsync(id);
        }

        public async Task AddAsync(Author author)
        {
            // Добавляем нового автора в контекст.
            _context.Authors.Add(author);
            // Сохраняем изменения в базе данных.
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Author author)
        {
            // Метод Update отслеживает все изменения в сущности и ее связях.
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            // Важно: логика проверки, можно ли удалять автора (например, если у него есть книги),
            // должна находиться в контроллере перед вызовом этого метода.
            var author = await _context.Authors.FindAsync(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
                await _context.SaveChangesAsync();
            }
        }
    }
}
