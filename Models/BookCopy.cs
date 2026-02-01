namespace ObreshkovLibrary.Models
{
    public class BookCopy
    {
        public int Id { get; set; }

        public int BookTitleId { get; set; }
        public BookTitle BookTitle { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }
}
