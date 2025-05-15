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
    public class JsonAuthorRepository : IAuthorRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public JsonAuthorRepository(IWebHostEnvironment webHostEnvironment)
        {
            // Убедитесь, что папка wwwroot/data существует
            var dataPath = Path.Combine(webHostEnvironment.WebRootPath, "data");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            _filePath = Path.Combine(dataPath, "authors.json");
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
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new List<Author>(), _options));
            }
        }

        private async Task<List<Author>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Author>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Author>();
            }
            return JsonSerializer.Deserialize<List<Author>>(json, _options) ?? new List<Author>();
        }

        private async Task WriteToFileAsync(List<Author> authors)
        {
            var json = JsonSerializer.Serialize(authors, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await ReadFromFileAsync();
        }

        public async Task<Author?> GetByIdAsync(Guid id)
        {
            var authors = await ReadFromFileAsync();
            return authors.FirstOrDefault(a => a.Id == id);
        }

        public async Task AddAsync(Author author)
        {
            var authors = await ReadFromFileAsync();
            authors.Add(author);
            await WriteToFileAsync(authors);
        }

        public async Task UpdateAsync(Author author)
        {
            var authors = await ReadFromFileAsync();
            var existingAuthor = authors.FirstOrDefault(a => a.Id == author.Id);
            if (existingAuthor != null)
            {
                existingAuthor.FirstName = author.FirstName;
                existingAuthor.LastName = author.LastName;
                existingAuthor.MiddleName = author.MiddleName;
                existingAuthor.DateOfBirth = author.DateOfBirth;
                await WriteToFileAsync(authors);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var authors = await ReadFromFileAsync();
            var authorToRemove = authors.FirstOrDefault(a => a.Id == id);
            if (authorToRemove != null)
            {
                authors.Remove(authorToRemove);
                await WriteToFileAsync(authors);
            }
        }
    }
}