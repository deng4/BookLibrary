using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookLibrary.Models;
using BookLibrary.Models.ViewModels;
using BookLibrary.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BookLibrary.Controllers
{
    public class ReadersController : Controller
    {
        private readonly IReaderRepository _readerRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository; // Для отображения авторов книг

        public ReadersController(IReaderRepository readerRepository, IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _readerRepository = readerRepository;
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }

        // GET: Readers
        public async Task<IActionResult> Index()
        {
            var readers = await _readerRepository.GetAllAsync();
            return View(readers.OrderBy(r => r.LastName).ThenBy(r => r.FirstName));
        }

        // GET: Readers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null)
            {
                return NotFound();
            }

            var allBooks = await _bookRepository.GetAllAsync();
            var borrowedBooksModels = allBooks.Where(b => b.CurrentReaderId == id.Value).ToList();

            var authors = await _authorRepository.GetAllAsync(); // Получаем всех авторов один раз

            var borrowedBooksViewModel = borrowedBooksModels.Select(b => new BookWithAuthorsViewModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorNames = b.AuthorIds?.Select(authorId =>
                {
                    var author = authors.FirstOrDefault(a => a.Id == authorId);
                    return author?.FullName ?? "Автор не найден";
                }).ToList() ?? new List<string>()
            }).ToList();

            var viewModel = new ReaderViewModel
            {
                Id = reader.Id,
                FirstName = reader.FirstName,
                LastName = reader.LastName,
                MiddleName = reader.MiddleName,
                Email = reader.Email,
                PhoneNumber = reader.PhoneNumber,
                RegistrationDate = reader.RegistrationDate,
                BorrowedBooks = borrowedBooksViewModel
            };

            return View(viewModel);
        }

        // GET: Readers/Create (Регистрация нового читателя)
        public IActionResult Create()
        {
            return View(new ReaderViewModel()); // Или Reader, если ViewModel не добавляет специфики
        }

        // POST: Readers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,MiddleName,Email,PhoneNumber")] ReaderViewModel readerViewModel)
        {
            if (ModelState.IsValid)
            {
                // Проверка на существующий Email, если это требуется
                var existingReader = (await _readerRepository.GetAllAsync()).FirstOrDefault(r => r.Email.Equals(readerViewModel.Email, StringComparison.OrdinalIgnoreCase));
                if (existingReader != null)
                {
                    ModelState.AddModelError("Email", "Читатель с таким Email уже зарегистрирован.");
                }
                else
                {
                    var reader = new Reader
                    {
                        FirstName = readerViewModel.FirstName,
                        LastName = readerViewModel.LastName,
                        MiddleName = readerViewModel.MiddleName,
                        Email = readerViewModel.Email,
                        PhoneNumber = readerViewModel.PhoneNumber,
                        RegistrationDate = DateTime.UtcNow
                        // Id генерируется в конструкторе Reader
                    };
                    await _readerRepository.AddAsync(reader);
                    TempData["SuccessMessage"] = $"Читатель '{reader.FullName}' успешно зарегистрирован.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(readerViewModel);
        }

        // GET: Readers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null)
            {
                return NotFound();
            }
            var readerViewModel = new ReaderViewModel // Используем ViewModel
            {
                Id = reader.Id,
                FirstName = reader.FirstName,
                LastName = reader.LastName,
                MiddleName = reader.MiddleName,
                Email = reader.Email,
                PhoneNumber = reader.PhoneNumber,
                RegistrationDate = reader.RegistrationDate // Не редактируется, но можно отобразить
            };
            return View(readerViewModel);
        }

        // POST: Readers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FirstName,LastName,MiddleName,Email,PhoneNumber")] ReaderViewModel readerViewModel)
        {
            if (id != readerViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Проверка на существующий Email (кроме текущего пользователя)
                var existingReaderWithEmail = (await _readerRepository.GetAllAsync())
                    .FirstOrDefault(r => r.Email.Equals(readerViewModel.Email, StringComparison.OrdinalIgnoreCase) && r.Id != readerViewModel.Id);
                if (existingReaderWithEmail != null)
                {
                    ModelState.AddModelError("Email", "Другой читатель с таким Email уже зарегистрирован.");
                }
                else
                {
                    try
                    {
                        var readerToUpdate = await _readerRepository.GetByIdAsync(id);
                        if (readerToUpdate == null) return NotFound();

                        readerToUpdate.FirstName = readerViewModel.FirstName;
                        readerToUpdate.LastName = readerViewModel.LastName;
                        readerToUpdate.MiddleName = readerViewModel.MiddleName;
                        readerToUpdate.Email = readerViewModel.Email;
                        readerToUpdate.PhoneNumber = readerViewModel.PhoneNumber;

                        await _readerRepository.UpdateAsync(readerToUpdate);
                        TempData["SuccessMessage"] = $"Данные читателя '{readerToUpdate.FullName}' успешно обновлены.";
                    }
                    catch (Exception)
                    {
                        if (await _readerRepository.GetByIdAsync(readerViewModel.Id) == null)
                        {
                            return NotFound();
                        }
                        ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении данных читателя.");
                        return View(readerViewModel);
                    }
                    return RedirectToAction(nameof(Details), new { id = readerViewModel.Id });
                }
            }
            return View(readerViewModel);
        }


        // GET: Readers/BorrowBook/readerId
        [HttpGet("Readers/BorrowBook/{readerId:guid}")]
        public async Task<IActionResult> BorrowBook(Guid readerId)
        {
            var reader = await _readerRepository.GetByIdAsync(readerId);
            if (reader == null) return NotFound();

            var allBooks = await _bookRepository.GetAllAsync();
            var availableBooks = allBooks.Where(b => !b.CurrentReaderId.HasValue)
                                        .OrderBy(b => b.Title)
                                        .Select(b => new SelectListItem
                                        {
                                            Value = b.Id.ToString(),
                                            Text = b.Title
                                        }).ToList();

            if (!availableBooks.Any())
            {
                TempData["InfoMessage"] = "Нет доступных книг для выдачи.";
            }

            var viewModel = new BorrowBookViewModel
            {
                ReaderId = readerId,
                ReaderName = reader.FullName,
                AvailableBooks = availableBooks
            };
            return View(viewModel);
        }

        [HttpPost("Readers/BorrowBook/{readerId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook(Guid readerId, BorrowBookViewModel viewModel)
        {
            if (readerId != viewModel.ReaderId) return BadRequest();

            var reader = await _readerRepository.GetByIdAsync(readerId);
            var book = await _bookRepository.GetByIdAsync(viewModel.SelectedBookId);

            if (reader == null || book == null) return NotFound();

            if (ModelState.IsValid) // SelectedBookId должен быть Required в ViewModel
            {
                if (book.CurrentReaderId.HasValue)
                {
                    ModelState.AddModelError("SelectedBookId", "Эта книга уже выдана другому читателю.");
                    TempData["ErrorMessage"] = "Выбранная книга уже выдана.";
                }
                else
                {
                    book.CurrentReaderId = readerId;
                    await _bookRepository.UpdateAsync(book);
                    TempData["SuccessMessage"] = $"Книга '{book.Title}' успешно выдана читателю {reader.FullName}.";
                    return RedirectToAction(nameof(Details), new { id = readerId });
                }
            }

            // Если ошибка, перезагружаем данные
            var allBooks = await _bookRepository.GetAllAsync();
            viewModel.AvailableBooks = allBooks.Where(b => !b.CurrentReaderId.HasValue)
                                            .OrderBy(b => b.Title)
                                            .Select(b => new SelectListItem
                                            {
                                                Value = b.Id.ToString(),
                                                Text = b.Title
                                            }).ToList();
            viewModel.ReaderName = reader.FullName; // Восстанавливаем имя
            return View(viewModel);
        }

        // POST: Readers/ReturnBook/bookId (для формы на странице деталей читателя)
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Route("Readers/ReturnBook/{bookId:guid}/{readerId:guid}")] // Можно использовать такой Route, если кнопка уникальна
        public async Task<IActionResult> ReturnBook(Guid bookId, Guid readerId) // readerId для редиректа и проверки
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            var reader = await _readerRepository.GetByIdAsync(readerId); // Для проверки и редиректа

            if (book == null || reader == null)
            {
                TempData["ErrorMessage"] = "Книга или читатель не найдены.";
                return RedirectToAction(nameof(Index)); // или на другую страницу ошибки
            }

            if (book.CurrentReaderId == null)
            {
                TempData["InfoMessage"] = $"Книга '{book.Title}' уже находится в библиотеке.";
            }
            else if (book.CurrentReaderId != readerId)
            {
                TempData["ErrorMessage"] = $"Книга '{book.Title}' не может быть возвращена этим читателем, так как она числится за другим.";
            }
            else
            {
                book.CurrentReaderId = null;
                await _bookRepository.UpdateAsync(book);
                TempData["SuccessMessage"] = $"Книга '{book.Title}' успешно возвращена в библиотеку.";
            }
            return RedirectToAction(nameof(Details), new { id = readerId });
        }


        // GET: Readers/Delete/5 (Опционально, т.к. обычно читателей не удаляют)
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null)
            {
                return NotFound();
            }

            // Проверка, есть ли у читателя книги на руках
            var books = await _bookRepository.GetAllAsync();
            if (books.Any(b => b.CurrentReaderId == id.Value))
            {
                ViewBag.HasBooks = true;
                TempData["ErrorMessage"] = $"Нельзя удалить читателя '{reader.FullName}', так как у него есть невозвращенные книги.";
            }

            return View(reader);
        }

        // POST: Readers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var reader = await _readerRepository.GetByIdAsync(id);
            if (reader == null)
            {
                return NotFound();
            }

            var books = await _bookRepository.GetAllAsync();
            if (books.Any(b => b.CurrentReaderId == id))
            {
                TempData["ErrorMessage"] = $"Нельзя удалить читателя '{reader.FullName}', так как у него есть невозвращенные книги.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            // Здесь IReaderRepository должен иметь метод DeleteAsync
            // await _readerRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Читатель '{reader.FullName}' успешно удален (если реализовано удаление).";
            // Если удаление не реализовано, замените на:
            // TempData["InfoMessage"] = "Функция удаления читателей не активна.";

            // Пока метод DeleteAsync не был определен для IReaderRepository в начальном запросе,
            // закомментируем его вызов. Если вы его добавите, раскомментируйте.
            if (_readerRepository is JsonReaderRepository jsonReaderRepo) // Проверка, если конкретный тип реализует Delete
            {
                // await jsonReaderRepo.DeleteAsync(id); // Если метод существует
                TempData["InfoMessage"] = $"Читатель '{reader.FullName}' успешно удален (если реализовано удаление).";
            }
            else
            {
                TempData["WarningMessage"] = "Удаление читателей не поддерживается текущей реализацией репозитория.";
            }


            return RedirectToAction(nameof(Index));
        }
    }
}