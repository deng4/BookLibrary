using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookLibrary.Models.ViewModels;
using BookLibrary.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using BookLibrary.Models.DatabaseModels; // Для List

namespace BookLibrary.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IReaderRepository _readerRepository;

        public BooksController(IBookRepository bookRepository, IAuthorRepository authorRepository, IReaderRepository readerRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _readerRepository = readerRepository;
        }

        // GET: Books
        // Исправлено: Проецируем Book в BookWithAuthorsViewModel
        public async Task<IActionResult> Index()
        {
            var books = await _bookRepository.GetAllAsync(); // Получаем все книги с включенными авторами и читателями

            // Проецируем каждую сущность Book в BookWithAuthorsViewModel
            var bookViewModels = books
                .OrderBy(b => b.Title) // Сначала сортируем
                .Select(book => new BookWithAuthorsViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    PublicationYear = book.PublicationYear,
                    ISBN = book.ISBN,
                    // Собираем имена авторов в List<string>
                    AuthorNames = book.Authors != null ? book.Authors.Select(a => a.FullName).ToList() : new List<string>(),
                    // Получаем имя текущего читателя, если книга у кого-то есть
                    CurrentReaderId = book.CurrentReaderId,
                    CurrentReaderName = book.CurrentReader != null ? book.CurrentReader.FullName : null
                })
                .ToList(); // Материализуем в список ViewModel'ей

            return View(bookViewModels);
        }

        // GET: Books/Details/5
        // Исправлено: Проецируем Book в BookWithAuthorsViewModel для детального просмотра
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var book = await _bookRepository.GetByIdAsync(id.Value); // Получаем книгу с включенными авторами и читателями
            if (book == null) return NotFound();

            // Проецируем полученную книгу в BookWithAuthorsViewModel
            var viewModel = new BookWithAuthorsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                PublicationYear = book.PublicationYear,
                ISBN = book.ISBN,
                AuthorNames = book.Authors != null ? book.Authors.Select(a => a.FullName).ToList() : new List<string>(),
                CurrentReaderId = book.CurrentReaderId,
                CurrentReaderName = book.CurrentReader != null ? book.CurrentReader.FullName : null
            };

            return View(viewModel);
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new BookViewModel
            {
                AvailableAuthors = await GetAuthorsSelectList()
            };
            return View(viewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (viewModel.SelectedAuthorIds == null || !viewModel.SelectedAuthorIds.Any())
                {
                    ModelState.AddModelError(nameof(viewModel.SelectedAuthorIds), "Необходимо указать хотя бы одного автора.");
                }
                else
                {
                    // Создаем новую книгу
                    var book = new Book
                    {
                        Id = Guid.NewGuid(),
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        PublicationYear = viewModel.PublicationYear,
                        ISBN = viewModel.ISBN
                    };

                    // Находим сущности авторов в БД по их ID
                    var allAuthors = await _authorRepository.GetAllAsync();
                    book.Authors = allAuthors.Where(a => viewModel.SelectedAuthorIds.Contains(a.Id)).ToList();

                    await _bookRepository.AddAsync(book);
                    TempData["SuccessMessage"] = $"Книга '{book.Title}' успешно добавлена.";
                    return RedirectToAction(nameof(Index));
                }
            }
            // Если модель невалидна, снова загружаем авторов для выпадающего списка
            viewModel.AvailableAuthors = await GetAuthorsSelectList();
            return View(viewModel);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null) return NotFound();

            var viewModel = new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                PublicationYear = book.PublicationYear,
                ISBN = book.ISBN,
                SelectedAuthorIds = book.Authors.Select(a => a.Id).ToList(),
                AvailableAuthors = await GetAuthorsSelectList()
            };
            return View(viewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BookViewModel viewModel)
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (viewModel.SelectedAuthorIds == null || !viewModel.SelectedAuthorIds.Any())
                {
                    ModelState.AddModelError(nameof(viewModel.SelectedAuthorIds), "Необходимо указать хотя бы одного автора.");
                }
                else
                {
                    // Получаем книгу из БД, включая ее текущих авторов
                    var bookToUpdate = await _bookRepository.GetByIdAsync(id);
                    if (bookToUpdate == null) return NotFound();

                    // Обновляем скалярные свойства
                    bookToUpdate.Title = viewModel.Title;
                    bookToUpdate.Description = viewModel.Description;
                    bookToUpdate.PublicationYear = viewModel.PublicationYear;
                    bookToUpdate.ISBN = viewModel.ISBN;

                    // Обновляем коллекцию авторов
                    var allAuthors = await _authorRepository.GetAllAsync();
                    var selectedAuthors = allAuthors.Where(a => viewModel.SelectedAuthorIds.Contains(a.Id)).ToList();
                    bookToUpdate.Authors = selectedAuthors;

                    try
                    {
                        await _bookRepository.UpdateAsync(bookToUpdate);
                        TempData["SuccessMessage"] = $"Книга '{bookToUpdate.Title}' успешно обновлена.";
                    }
                    catch (Exception)
                    {
                        if (await _bookRepository.GetByIdAsync(id) == null) return NotFound();
                        throw;
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            // Если модель невалидна, перезагружаем авторов
            viewModel.AvailableAuthors = await GetAuthorsSelectList();
            return View(viewModel);
        }

        // GET: Books/Delete/5
        // Исправлено: Проецируем Book в BookWithAuthorsViewModel для удаления
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null) return NotFound();

            // Проецируем книгу в BookWithAuthorsViewModel перед передачей в представление
            var viewModel = new BookWithAuthorsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                PublicationYear = book.PublicationYear,
                ISBN = book.ISBN,
                AuthorNames = book.Authors != null ? book.Authors.Select(a => a.FullName).ToList() : new List<string>(),
                CurrentReaderId = book.CurrentReaderId,
                CurrentReaderName = book.CurrentReader != null ? book.CurrentReader.FullName : null
            };

            if (book.CurrentReaderId.HasValue)
            {
                ViewBag.ErrorMessage = "Нельзя удалить книгу, которая находится у читателя.";
            }

            return View(viewModel); // Передаем ViewModel
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();

            if (book.CurrentReaderId.HasValue)
            {
                TempData["ErrorMessage"] = "Нельзя удалить книгу, которая находится у читателя.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _bookRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Книга '{book.Title}' успешно удалена.";
            return RedirectToAction(nameof(Index));
        }

        // Вспомогательный метод для получения списка авторов
        private async Task<IEnumerable<SelectListItem>> GetAuthorsSelectList()
        {
            var authors = await _authorRepository.GetAllAsync();
            return authors
                .OrderBy(a => a.FullName)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName
                }).ToList();
        }
    }
}