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
    public class JsonReaderRepository /*: IReaderRepository*/
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _options;

        public JsonReaderRepository(IWebHostEnvironment webHostEnvironment)
        {
            // Убедитесь, что папка wwwroot/data существует
            var dataPath = Path.Combine(webHostEnvironment.WebRootPath, "data");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            _filePath = Path.Combine(dataPath, "readers.json");
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
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new List<Reader>(), _options));
            }
        }

        private async Task<List<Reader>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Reader>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Reader>();
            }
            return JsonSerializer.Deserialize<List<Reader>>(json, _options) ?? new List<Reader>();
        }

        private async Task WriteToFileAsync(List<Reader> readers)
        {
            var json = JsonSerializer.Serialize(readers, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<IEnumerable<Reader>> GetAllAsync()
        {
            return await ReadFromFileAsync();
        }

        public async Task<Reader?> GetByIdAsync(Guid id)
        {
            var authors = await ReadFromFileAsync();
            return authors.FirstOrDefault(a => a.Id == id);
        }

        public async Task AddAsync(Reader reader)
        {
            var readers = await ReadFromFileAsync();
            readers.Add(reader);
            await WriteToFileAsync(readers);
        }

        public async Task UpdateAsync(Reader reader)
        {
            var readers = await ReadFromFileAsync();
            var existingReader = readers.FirstOrDefault(a => a.Id == reader.Id);
            if (existingReader != null)
            {
                existingReader.FirstName = reader.FirstName;
                existingReader.LastName = reader.LastName;
                existingReader.MiddleName = reader.MiddleName;
                existingReader.Email = reader.Email;
                existingReader.PhoneNumber = reader.PhoneNumber;
                existingReader.RegistrationDate = reader.RegistrationDate;
                await WriteToFileAsync(readers);
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var readers = await ReadFromFileAsync();
            var readerToRemove = readers.FirstOrDefault(a => a.Id == id);
            if (readerToRemove != null)
            {
                readers.Remove(readerToRemove);
                await WriteToFileAsync(readers);
            }
        }
    }
}