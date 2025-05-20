using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookLibrary.Models.ViewModels
{
    public class BorrowBookViewModel
    {
        [Required]
        public Guid ReaderId { get; set; }

        [Required(ErrorMessage = "Пожалуйста, выберите книгу.")]
        [Display(Name = "Книга для выдачи")]
        public Guid SelectedBookId { get; set; }

        public IEnumerable<SelectListItem>? AvailableBooks { get; set; } // Список доступных книг
        public string? ReaderName { get; set; } // Для отображения имени читателя на форме
    }
}