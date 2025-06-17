using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookLibrary.Models.ViewModels; // Если вы создали AuthorViewModel
using BookLibrary.Repositories;
using BookLibrary.Models.DatabaseModels;

namespace BookLibrary.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository bookRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            var authors = await _authorRepository.GetAllAsync();
            return View(authors.OrderBy(a => a.LastName).ThenBy(a => a.FirstName));
        }

        // GET: Authors/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();
            var author = await _authorRepository.GetByIdAsync(id.Value);
            if (author == null) return NotFound();
            return View(author);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,MiddleName,DateOfBirth")] Author author)
        {
            if (ModelState.IsValid)
            {
                author.Id = Guid.NewGuid();
                await _authorRepository.AddAsync(author);
                TempData["SuccessMessage"] = $"Автор '{author.FullName}' успешно добавлен.";
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var author = await _authorRepository.GetByIdAsync(id.Value);
            if (author == null) return NotFound();
            return View(author);
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FirstName,LastName,MiddleName,DateOfBirth")] Author author)
        {
            if (id != author.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _authorRepository.UpdateAsync(author);
                    TempData["SuccessMessage"] = $"Данные автора '{author.FullName}' успешно обновлены.";
                }
                catch (Exception)
                {
                    // Проверяем, существует ли автор, если произошла ошибка
                    if (await _authorRepository.GetByIdAsync(id) == null)
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var author = await _authorRepository.GetByIdAsync(id.Value);
            if (author == null) return NotFound();

            // Проверка, есть ли у автора книги
            var allBooks = await _bookRepository.GetAllAsync();
            if (allBooks.Any(b => b.Authors.Any(a => a.Id == id.Value)))
            {
                ViewBag.ErrorMessage = $"Нельзя удалить автора '{author.FullName}', так как за ним числятся книги.";
            }

            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null) return NotFound();

            // Повторная проверка перед удалением
            var allBooks = await _bookRepository.GetAllAsync();
            if (allBooks.Any(b => b.Authors.Any(a => a.Id == id)))
            {
                TempData["ErrorMessage"] = $"Нельзя удалить автора '{author.FullName}', так как за ним числятся книги.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _authorRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Автор '{author.FullName}' успешно удален.";
            return RedirectToAction(nameof(Index));
        }
    }
}