using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BookLibrary.Models;
using Microsoft.AspNetCore.Hosting;

namespace BookLibrary.Repositories
{
    public class JsonBookRepository /*: IBookRepository*/
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public JsonBookRepository(IWebHostEnvironment webHostEnvironment)
        {
            // Убедитесь, что папка wwwroot/data существует
            var dataPath = Path.Combine(webHostEnvironment.WebRootPath, "data");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            _filePath = Path.Combine(dataPath, "books.json");
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            InitializeFile();
        }

        private void InitializeFile()
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new List<Book>(), _options));
            }
        }

        private async Task<List<Book>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Book>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Book>();
            }
            return JsonSerializer.Deserialize<List<Book>>(json, _options) ?? new List<Book>();
        }

        private async Task WriteToFileAsync(List<Book> books)
        {
            var json = JsonSerializer.Serialize(books, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await ReadFromFileAsync();
        }

        public async Task<Book?> GetByIdAsync(Guid id)
        {
            var books = await ReadFromFileAsync();
            return books.FirstOrDefault(a => a.Id == id);
        }

        public async Task AddAsync(Book book)
        {
            var books = await ReadFromFileAsync();
            books.Add(book);
            await WriteToFileAsync(books);
        }

        public async Task UpdateAsync(Book book)
        {
            var books = await ReadFromFileAsync();
            var existingBook = books.FirstOrDefault(a => a.Id == book.Id);
            if (existingBook != null)
            {
                existingBook.Title = book.Title;
                existingBook.Description = book.Description;
                existingBook.AuthorIds = book.AuthorIds;
                existingBook.PublicationYear = book.PublicationYear;
                existingBook.ISBN = book.ISBN;
                existingBook.CurrentReaderId = book.CurrentReaderId;
                await WriteToFileAsync(books);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var books = await ReadFromFileAsync();
            var bookToRemove = books.FirstOrDefault(a => a.Id == id);
            if (bookToRemove != null)
            {
                books.Remove(bookToRemove);
                await WriteToFileAsync(books);
            }
        }

        public async Task<IEnumerable<Book>> GetBooksByReaderIdAsync(Guid readerId)
        {
            var books = await ReadFromFileAsync();
            return books.Where(a=>a.Id== readerId).ToList();
        }
    }
}