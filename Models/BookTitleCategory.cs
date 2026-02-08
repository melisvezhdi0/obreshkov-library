namespace ObreshkovLibrary.Models
{
    public class BookTitleCategory
    {
        public int BookTitleId { get; set; }
        public BookTitle BookTitle { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }
}
