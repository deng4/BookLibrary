using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookLibrary.Models;
using BookLibrary.Models.ViewModels; // Если вы создали AuthorViewModel
using BookLibrary.Repositories;

namespace BookLibrary.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository; // Для проверки перед удалением

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
            if (id == null)
            {
                return NotFound();
            }

            var author = await _authorRepository.GetByIdAsync(id.Value);
            if (author == null)
            {
                return NotFound();
            }

            // Опционально: показать книги этого автора
            // var books = (await _bookRepository.GetAllAsync()).Where(b => b.AuthorIds.Contains(author.Id));
            // ViewBag.BooksByAuthor = books;

            return View(author);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            // Используем AuthorViewModel если он отличается от Author, иначе можно Author
            return View(new AuthorViewModel());
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,MiddleName,DateOfBirth")] AuthorViewModel authorViewModel)
        // или public async Task<IActionResult> Create(Author author) если не используете ViewModel
        {
            if (ModelState.IsValid)
            {
                var author = new Author
                {
                    FirstName = authorViewModel.FirstName,
                    LastName = authorViewModel.LastName,
                    MiddleName = authorViewModel.MiddleName,
                    DateOfBirth = authorViewModel.DateOfBirth
                    // Id генерируется в конструкторе Author
                };
                await _authorRepository.AddAsync(author);
                TempData["SuccessMessage"] = $"Автор '{author.FullName}' успешно добавлен.";
                return RedirectToAction(nameof(Index));
            }
            return View(authorViewModel);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _authorRepository.GetByIdAsync(id.Value);
            if (author == null)
            {
                return NotFound();
            }
            // Преобразование в ViewModel, если используется
            var authorViewModel = new AuthorViewModel
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                MiddleName = author.MiddleName,
                DateOfBirth = author.DateOfBirth
            };
            return View(authorViewModel);
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FirstName,LastName,MiddleName,DateOfBirth")] AuthorViewModel authorViewModel)
        {
            if (id != authorViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var author = new Author
                    {
                        Id = authorViewModel.Id,
                        FirstName = authorViewModel.FirstName,
                        LastName = authorViewModel.LastName,
                        MiddleName = authorViewModel.MiddleName,
                        DateOfBirth = authorViewModel.DateOfBirth
                    };
                    await _authorRepository.UpdateAsync(author);
                    TempData["SuccessMessage"] = $"Данные автора '{author.FullName}' успешно обновлены.";
                }
                catch (Exception) // Можно уточнить тип исключения (e.g., KeyNotFoundException if update implies existence)
                {
                    if (await _authorRepository.GetByIdAsync(authorViewModel.Id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        // Логирование ошибки
                        ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении данных автора.");
                        return View(authorViewModel);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(authorViewModel);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var author = await _authorRepository.GetByIdAsync(id.Value);
            if (author == null)
            {
                return NotFound();
            }

            // Проверка, есть ли у автора книги
            var books = await _bookRepository.GetAllAsync();
            if (books.Any(b => b.AuthorIds.Contains(id.Value)))
            {
                ViewBag.HasBooks = true; // Передаем флаг в представление
                TempData["ErrorMessage"] = $"Нельзя удалить автора '{author.FullName}', так как за ним числятся книги. Сначала удалите или измените книги этого автора.";
            }


            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            var books = await _bookRepository.GetAllAsync();
            if (books.Any(b => b.AuthorIds.Contains(id)))
            {
                TempData["ErrorMessage"] = $"Нельзя удалить автора '{author.FullName}', так как за ним числятся книги.";
                // Перенаправляем обратно на страницу удаления, где отобразится сообщение
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            await _authorRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Автор '{author.FullName}' успешно удален.";
            return RedirectToAction(nameof(Index));
        }
    }
}