namespace BookLibrary.Models.ViewModels
{
    // Дополнительная ViewModel для отображения книг с авторами
    public class BookWithAuthorsViewModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string Title { get; set; } = string.Empty; // Инициализация, чтобы избежать NRE
        public List<string> AuthorNames { get; set; } = new List<string>();
        public string? CurrentReaderName { get; set; } // Если нужно показать, кто взял
        public Guid? CurrentReaderId { get; set; }
        public string? ISBN { get; set; }
        public int? PublicationYear { get; set; }
    }
}