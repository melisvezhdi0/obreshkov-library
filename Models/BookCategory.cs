namespace ObreshkovLibrary.Models
{
    public class BookCategory
    {
        public int BookTitleId { get; set; }
        public Book BookTitle { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
