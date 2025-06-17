using System.ComponentModel.DataAnnotations;

namespace BookLibrary.Models.DatabaseModels
{
    public class Author
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Имя автора обязательно для заполнения")]
        [Display(Name = "Имя")]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия автора обязательна для заполнения")]
        [Display(Name = "Фамилия")]
        [StringLength(100)]
        public string LastName { get; set; }

        [Display(Name = "Отчество")]
        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Display(Name = "Дата рождения")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        // Навигационное свойство для связи "многие-ко-многим"
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
