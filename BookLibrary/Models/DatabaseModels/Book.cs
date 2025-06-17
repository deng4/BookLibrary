using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookLibrary.Models.DatabaseModels
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Название книги обязательно для заполнения")]
        [Display(Name = "Название")]
        [StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Год публикации")]
        public int? PublicationYear { get; set; }

        [Display(Name = "ISBN")]
        [StringLength(20)]
        public string? ISBN { get; set; }

        // Связь с читателем (один-ко-многим)
        public Guid? CurrentReaderId { get; set; }
        [ForeignKey("CurrentReaderId")]
        public virtual Reader? CurrentReader { get; set; }

        // Навигационное свойство для связи "многие-ко-многим"
        public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
    }
}
