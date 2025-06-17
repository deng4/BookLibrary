using BookLibrary.Models.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


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