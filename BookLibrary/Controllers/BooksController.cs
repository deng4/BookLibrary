using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic; // Для List

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
        public async Task<IActionResult> Index()
        {
            var books = await _bookRepository.GetAllAsync();
            var authors = await _authorRepository.GetAllAsync();
            var readers = await _readerRepository.GetAllAsync();

            var bookViewModels = books.Select(b => new BookWithAuthorsViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorNames = b.AuthorIds.Select(authorId =>
                {
                    var author = authors.FirstOrDefault(a => a.Id == authorId);
                    return author?.FullName ?? "Неизвестный автор";
                }).ToList(),
                CurrentReaderId = b.CurrentReaderId,
                CurrentReaderName = b.CurrentReaderId.HasValue ? readers.FirstOrDefault(r => r.Id == b.CurrentReaderId.Value)?.FullName : "В библиотеке"
            }).ToList();

            return View(bookViewModels);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            var authors = await _authorRepository.GetAllAsync();
            var reader = book.CurrentReaderId.HasValue ? await _readerRepository.GetByIdAsync(book.CurrentReaderId.Value) : null;

            var viewModel = new BookWithAuthorsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                // Description = book.Description, // Добавьте, если нужно
                // PublicationYear = book.PublicationYear, // Добавьте, если нужно
                // ISBN = book.ISBN, // Добавьте, если нужно
                AuthorNames = book.AuthorIds.Select(authorId =>
                {
                    var author = authors.FirstOrDefault(a => a.Id == authorId);
                    return author?.FullName ?? "Неизвестный автор";
                }).ToList(),
                CurrentReaderId = book.CurrentReaderId,
                CurrentReaderName = reader?.FullName ?? "В библиотеке"
            };

            return View(viewModel);
        }

        // GET: Books/Create
        public async Task<IActionResult> Create()
        {
            var authors = await _authorRepository.GetAllAsync();
            var viewModel = new BookViewModel
            {
                AvailableAuthors = authors.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName
                }).ToList()
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
                    ModelState.AddModelError("SelectedAuthorIds", "Необходимо указать хотя бы одного автора.");
                }

                if (ModelState.IsValid) // Проверяем еще раз после добавления ошибки
                {
                    var book = new Book
                    {
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        AuthorIds = viewModel.SelectedAuthorIds,
                        PublicationYear = viewModel.PublicationYear,
                        ISBN = viewModel.ISBN
                    };
                    await _bookRepository.AddAsync(book);
                    return RedirectToAction(nameof(Index));
                }
            }
            // Если модель невалидна, снова загружаем авторов для выпадающего списка
            var authors = await _authorRepository.GetAllAsync();
            viewModel.AvailableAuthors = authors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.FullName
            }).ToList();
            return View(viewModel);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            var authors = await _authorRepository.GetAllAsync();
            var viewModel = new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                SelectedAuthorIds = book.AuthorIds,
                PublicationYear = book.PublicationYear,
                ISBN = book.ISBN,
                AvailableAuthors = authors.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.FullName,
                    Selected = book.AuthorIds.Contains(a.Id)
                }).ToList()
            };
            return View(viewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BookViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (viewModel.SelectedAuthorIds == null || !viewModel.SelectedAuthorIds.Any())
                {
                    ModelState.AddModelError("SelectedAuthorIds", "Необходимо указать хотя бы одного автора.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var book = new Book
                        {
                            Id = viewModel.Id,
                            Title = viewModel.Title,
                            Description = viewModel.Description,
                            AuthorIds = viewModel.SelectedAuthorIds,
                            PublicationYear = viewModel.PublicationYear,
                            ISBN = viewModel.ISBN
                            // CurrentReaderId не меняем здесь, это делается через "взять/вернуть"
                        };
                        // Важно: нужно получить существующую книгу, чтобы не потерять CurrentReaderId
                        var existingBook = await _bookRepository.GetByIdAsync(id);
                        if (existingBook == null) return NotFound();
                        book.CurrentReaderId = existingBook.CurrentReaderId;

                        await _bookRepository.UpdateAsync(book);
                    }
                    catch (Exception) // Можно уточнить тип исключения
                    {
                        // Логирование ошибки или обработка ситуации, когда книга не найдена для обновления
                        if (await _bookRepository.GetByIdAsync(viewModel.Id) == null)
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            var authors = await _authorRepository.GetAllAsync();
            viewModel.AvailableAuthors = authors.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.FullName
            }).ToList();
            return View(viewModel);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookRepository.GetByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }
            // Для отображения информации перед удалением
            var authors = await _authorRepository.GetAllAsync();
            var reader = book.CurrentReaderId.HasValue ? await _readerRepository.GetByIdAsync(book.CurrentReaderId.Value) : null;

            var viewModel = new BookWithAuthorsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                AuthorNames = book.AuthorIds.Select(authorId =>
                {
                    var author = authors.FirstOrDefault(a => a.Id == authorId);
                    return author?.FullName ?? "Неизвестный автор";
                }).ToList(),
                CurrentReaderName = reader?.FullName ?? "В библиотеке"
            };


            return View(viewModel); // Используем viewModel для подтверждения
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book != null && book.CurrentReaderId.HasValue)
            {
                // Можно добавить логику: не удалять, если книга на руках, или сначала требовать вернуть
                ModelState.AddModelError(string.Empty, "Нельзя удалить книгу, которая находится у читателя.");
                // Перезагрузить данные для View
                var authors = await _authorRepository.GetAllAsync();
                var reader = await _readerRepository.GetByIdAsync(book.CurrentReaderId.Value);
                var viewModel = new BookWithAuthorsViewModel { /* ... заполнить ... */ };
                return View(viewModel);
            }
            await _bookRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}