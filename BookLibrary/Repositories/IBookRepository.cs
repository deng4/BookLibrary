using BookLibrary.Models.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace BookLibrary.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(Guid id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Book>> GetBooksByReaderIdAsync(Guid readerId);
    }
}