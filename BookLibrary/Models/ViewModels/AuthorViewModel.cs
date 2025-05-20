using System;
using System.ComponentModel.DataAnnotations;

namespace BookLibrary.Models.ViewModels
{
    public class AuthorViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Имя автора обязательно для заполнения")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия автора обязательна для заполнения")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        public string? MiddleName { get; set; }

        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}