using BookLibrary.Models.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace BookLibrary.Repositories
{
    public interface IAuthorRepository
    {
        Task<IEnumerable<Author>> GetAllAsync();
        Task<Author?> GetByIdAsync(Guid id);
        Task AddAsync(Author author);
        Task UpdateAsync(Author author);
        Task DeleteAsync(Guid id);
    }
}