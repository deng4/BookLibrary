using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookLibrary.Models
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Название книги обязательно для заполнения")]
        [Display(Name = "Название")]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Необходимо указать хотя бы одного автора")]
        [Display(Name = "Авторы")]
        public List<Guid> AuthorIds { get; set; } = new List<Guid>();

        [Display(Name = "Год публикации")]
        public int? PublicationYear { get; set; }

        [Display(Name = "ISBN")]
        public string? ISBN { get; set; }

        public Guid? CurrentReaderId { get; set; } // ID читателя, который взял книгу
    }
}