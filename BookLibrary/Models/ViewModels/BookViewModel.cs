using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Для SelectList

namespace BookLibrary.Models.ViewModels
{
    public class BookViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Название книги обязательно для заполнения")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Необходимо указать хотя бы одного автора")]
        [Display(Name = "Авторы")]
        public List<Guid> SelectedAuthorIds { get; set; } = new List<Guid>();

        public IEnumerable<SelectListItem>? AvailableAuthors { get; set; } // Для выпадающего списка

        [Display(Name = "Год публикации")]
        [Range(1, 2025, ErrorMessage = "Год публикации должен быть корректным")] // 2025 - текущий год
        public int? PublicationYear { get; set; }

        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }
    }
}