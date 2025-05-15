using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookLibrary.Models;

namespace BookLibrary.Repositories
{
    public interface IReaderRepository
    {
        Task<IEnumerable<Reader>> GetAllAsync();
        Task<Reader?> GetByIdAsync(Guid id);
        Task AddAsync(Reader reader);
        Task UpdateAsync(Reader reader);
        Task DeleteAsync(Guid id); // Опционально, обычно читателей не удаляют
    }
}