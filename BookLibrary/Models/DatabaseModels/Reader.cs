using System.ComponentModel.DataAnnotations;

namespace BookLibrary.Models.DatabaseModels
{
    public class Reader
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Имя читателя обязательно для заполнения")]
        [Display(Name = "Имя")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия читателя обязательна для заполнения")]
        [Display(Name = "Фамилия")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        [StringLength(150)]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [Display(Name = "Телефон")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        // Навигационное свойство для книг, которые взял читатель
        public virtual ICollection<Book> BorrowedBooks { get; set; } = new List<Book>();
    }
}
