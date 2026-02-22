namespace ObreshkovLibrary.Models
{
    public class BookCopy
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }
}