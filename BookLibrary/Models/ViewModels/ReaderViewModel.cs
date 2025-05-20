using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BookLibrary.Models; // Для списка книг

namespace BookLibrary.Models.ViewModels
{
    public class ReaderViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Имя читателя обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия читателя обязательна для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime RegistrationDate { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        public IEnumerable<BookWithAuthorsViewModel>? BorrowedBooks { get; set; } // Книги, взятые читателем
    }

    // Дополнительная ViewModel для отображения книг с авторами
    public class BookWithAuthorsViewModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string Title { get; set; }
        public List<string> AuthorNames { get; set; } = new List<string>();
        public string? CurrentReaderName { get; set; } // Если нужно показать, кто взял
        public Guid? CurrentReaderId { get; set; }
        public string? ISBN { get; set; }
        public int? PublicationYear { get; set; }
    }
}