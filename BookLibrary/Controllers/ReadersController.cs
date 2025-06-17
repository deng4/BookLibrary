using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectListItem
using Microsoft.EntityFrameworkCore; // Для DbUpdateException
using Microsoft.Data.SqlClient; // <--- ДОБАВЛЕНО для SqlException
using System.Collections.Generic; // Для IEnumerable, List
using BookLibrary.Models.ViewModels; // Для всех ViewModel
using BookLibrary.Repositories; // Для интерфейсов репозиториев
using BookLibrary.Models.DatabaseModels; // Для сущностей БД Reader

namespace BookLibrary.Controllers
{
    public class ReadersController : Controller
    {
        private readonly IReaderRepository _readerRepository;
        private readonly IBookRepository _bookRepository;

        public ReadersController(IReaderRepository readerRepository, IBookRepository bookRepository)
        {
            _readerRepository = readerRepository;
            _bookRepository = bookRepository;
        }

        // GET: Readers
        public async Task<IActionResult> Index()
        {
            var readers = await _readerRepository.GetAllAsync(); // Получаем всех читателей (DatabaseModels.Reader)

            // Проецируем каждую сущность Reader в ReaderViewModel
            var readerViewModels = readers
                .OrderBy(r => r.LastName)
                .Select(r => new ReaderViewModel // Проецируем в ожидаемый ViewModel
                {
                    Id = r.Id,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    MiddleName = r.MiddleName,
                    Email = r.Email,
                    PhoneNumber = r.PhoneNumber,
                    RegistrationDate = r.RegistrationDate,
                    // Добавляем счетчик книг, взятых читателем
                    BorrowedBooksCount = r.BorrowedBooks?.Count ?? 0
                })
                .ToList();

            return View(readerViewModels);
        }

        // GET: Readers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();
            var reader = await _readerRepository.GetByIdAsync(id.Value); // Получаем читателя с подгруженными книгами
            if (reader == null) return NotFound();

            // Проецируем читателя в ReaderViewModel
            var viewModel = new ReaderViewModel
            {
                Id = reader.Id,
                FirstName = reader.FirstName,
                LastName = reader.LastName,
                MiddleName = reader.MiddleName,
                Email = reader.Email,
                PhoneNumber = reader.PhoneNumber,
                RegistrationDate = reader.RegistrationDate,
                BorrowedBooksCount = reader.BorrowedBooks?.Count ?? 0,
                // Проецируем каждую взятую книгу в BookWithAuthorsViewModel, если нужно показать их детали
                BorrowedBooks = reader.BorrowedBooks?
                                      .Select(b => new BookWithAuthorsViewModel
                                      {
                                          Id = b.Id,
                                          Title = b.Title,
                                          ISBN = b.ISBN,
                                          AuthorNames = b.Authors != null ? b.Authors.Select(a => a.FullName).ToList() : new List<string>()
                                      }).ToList()
                                      ?? new List<BookWithAuthorsViewModel>()
            };
            return View(viewModel);
        }

        // GET: Readers/Create
        public IActionResult Create()
        {
            // Для создания, представление может ожидать ReaderViewModel
            return View(new ReaderViewModel());
        }

        // POST: Readers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,MiddleName,Email,PhoneNumber")] ReaderViewModel viewModel) // Принимаем ReaderViewModel
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Преобразуем ViewModel в сущность базы данных Reader
                    var reader = new Reader // Это BookLibrary.Models.DatabaseModels.Reader
                    {
                        Id = Guid.NewGuid(),
                        FirstName = viewModel.FirstName,
                        LastName = viewModel.LastName,
                        MiddleName = viewModel.MiddleName,
                        Email = viewModel.Email,
                        PhoneNumber = viewModel.PhoneNumber,
                        RegistrationDate = DateTime.UtcNow // Устанавливаем дату регистрации
                    };

                    await _readerRepository.AddAsync(reader);
                    TempData["SuccessMessage"] = $"Читатель '{reader.FullName}' успешно зарегистрирован.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                var sqlException = ex.InnerException as Microsoft.Data.SqlClient.SqlException; // <--- ИЗМЕНЕНО
                if (sqlException != null && (sqlException.Number == 2627 || sqlException.Number == 2601)) // 2627: PK violation, 2601: Unique constraint violation
                {
                    // Отлов ошибки уникальности Email (или других уникальных полей) из БД
                    if (sqlException.Message.Contains("IX_Readers_Email") || sqlException.Message.Contains("Cannot insert duplicate key row"))
                    {
                        ModelState.AddModelError("Email", "Читатель с таким Email уже существует.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Произошла ошибка базы данных при добавлении читателя. Пожалуйста, попробуйте снова.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка.");
                }
            }
            // Если модель невалидна или произошла ошибка, возвращаем ViewModel
            return View(viewModel);
        }

        // GET: Readers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();
            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null) return NotFound();

            // Проецируем читателя в ReaderViewModel для редактирования
            var viewModel = new ReaderViewModel
            {
                Id = reader.Id,
                FirstName = reader.FirstName,
                LastName = reader.LastName,
                MiddleName = reader.MiddleName,
                Email = reader.Email,
                PhoneNumber = reader.PhoneNumber,
                RegistrationDate = reader.RegistrationDate // Сохраняем существующую дату регистрации
            };
            return View(viewModel);
        }

        // POST: Readers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FirstName,LastName,MiddleName,Email,PhoneNumber,RegistrationDate")] ReaderViewModel viewModel) // Принимаем ReaderViewModel
        {
            if (id != viewModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Получаем существующую сущность Reader из БД для обновления
                var readerToUpdate = await _readerRepository.GetByIdAsync(id);
                if (readerToUpdate == null) return NotFound();

                // Обновляем свойства сущности из ViewModel
                readerToUpdate.FirstName = viewModel.FirstName;
                readerToUpdate.LastName = viewModel.LastName;
                readerToUpdate.MiddleName = viewModel.MiddleName;
                readerToUpdate.Email = viewModel.Email;
                readerToUpdate.PhoneNumber = viewModel.PhoneNumber;
                readerToUpdate.RegistrationDate = viewModel.RegistrationDate; // Сохраняем дату регистрации

                try
                {
                    await _readerRepository.UpdateAsync(readerToUpdate);
                    TempData["SuccessMessage"] = $"Данные читателя '{readerToUpdate.FullName}' успешно обновлены.";
                    return RedirectToAction(nameof(Details), new { id = readerToUpdate.Id });
                }
                catch (DbUpdateException ex)
                {
                    var sqlException = ex.InnerException as Microsoft.Data.SqlClient.SqlException; // <--- ИЗМЕНЕНО
                    if (sqlException != null && (sqlException.Number == 2627 || sqlException.Number == 2601))
                    {
                        if (sqlException.Message.Contains("IX_Readers_Email") || sqlException.Message.Contains("Cannot insert duplicate key row"))
                        {
                            ModelState.AddModelError("Email", "Читатель с таким Email уже существует.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Произошла ошибка базы данных при обновлении читателя. Пожалуйста, попробуйте снова.");
                        }
                    }
                    else if (await _readerRepository.GetByIdAsync(id) == null) // Проверяем, был ли удален читатель другим запросом
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Произошла непредвиденная ошибка.");
                    }
                }
            }
            return View(viewModel); // Возвращаем ViewModel
        }

        // GET: Readers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();
            var reader = await _readerRepository.GetByIdAsync(id.Value);
            if (reader == null) return NotFound();

            // Проецируем читателя в ReaderViewModel для отображения в подтверждении удаления
            var viewModel = new ReaderViewModel
            {
                Id = reader.Id,
                FirstName = reader.FirstName,
                LastName = reader.LastName,
                MiddleName = reader.MiddleName,
                Email = reader.Email,
                PhoneNumber = reader.PhoneNumber,
                RegistrationDate = reader.RegistrationDate,
                BorrowedBooksCount = reader.BorrowedBooks?.Count ?? 0 // Подсчитываем книги для отображения предупреждения
            };

            // Проверка, есть ли у читателя книги на руках
            if (reader.BorrowedBooks != null && reader.BorrowedBooks.Any())
            {
                ViewBag.ErrorMessage = $"Нельзя удалить читателя '{reader.FullName}', так как у него есть невозвращенные книги.";
            }

            return View(viewModel); // Передаем ViewModel
        }

        // POST: Readers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var reader = await _readerRepository.GetByIdAsync(id);
            if (reader == null) return NotFound();

            if (reader.BorrowedBooks != null && reader.BorrowedBooks.Any())
            {
                TempData["ErrorMessage"] = $"Нельзя удалить читателя '{reader.FullName}', так как у него есть невозвращенные книги.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            await _readerRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = $"Читатель '{reader.FullName}' успешно удален.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Readers/BorrowBook/readerId
        [HttpGet("Readers/BorrowBook/{readerId:guid}")]
        public async Task<IActionResult> BorrowBook(Guid readerId)
        {
            var reader = await _readerRepository.GetByIdAsync(readerId);
            if (reader == null) return NotFound();

            var allBooks = await _bookRepository.GetAllAsync();
            var availableBooks = allBooks
                .Where(b => !b.CurrentReaderId.HasValue)
                .OrderBy(b => b.Title)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Title
                }).ToList();

            var viewModel = new BorrowBookViewModel
            {
                ReaderId = readerId,
                ReaderName = reader.FullName,
                AvailableBooks = availableBooks
            };

            if (!availableBooks.Any())
            {
                TempData["InfoMessage"] = "Нет доступных книг для выдачи.";
            }

            return View(viewModel);
        }

        // POST: Readers/BorrowBook/{readerId:guid}
        [HttpPost("Readers/BorrowBook/{readerId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook(Guid readerId, BorrowBookViewModel viewModel)
        {
            if (readerId != viewModel.ReaderId) return BadRequest();

            var reader = await _readerRepository.GetByIdAsync(readerId);
            var book = await _bookRepository.GetByIdAsync(viewModel.SelectedBookId);

            if (reader == null || book == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (book.CurrentReaderId.HasValue)
                {
                    ModelState.AddModelError("SelectedBookId", "Эта книга уже выдана другому читателю.");
                }
                else
                {
                    book.CurrentReaderId = readerId;
                    await _bookRepository.UpdateAsync(book);
                    TempData["SuccessMessage"] = $"Книга '{book.Title}' успешно выдана читателю {reader.FullName}.";
                    return RedirectToAction(nameof(Details), new { id = readerId });
                }
            }

            // Если ошибка, перезагружаем данные для View
            var allBooks = await _bookRepository.GetAllAsync();
            viewModel.AvailableBooks = allBooks.Where(b => !b.CurrentReaderId.HasValue).OrderBy(b => b.Title).Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Title }).ToList();
            viewModel.ReaderName = reader.FullName;
            return View(viewModel);
        }

        // POST: Readers/ReturnBook
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(Guid bookId, Guid readerId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null) return NotFound();

            if (book.CurrentReaderId != readerId)
            {
                TempData["ErrorMessage"] = "Ошибка: книга не числится за этим читателем.";
            }
            else
            {
                book.CurrentReaderId = null;
                await _bookRepository.UpdateAsync(book);
                TempData["SuccessMessage"] = $"Книга '{book.Title}' успешно возвращена в библиотеку.";
            }
            return RedirectToAction(nameof(Details), new { id = readerId });
        }
    }
}
