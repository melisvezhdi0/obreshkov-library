namespace ObreshkovLibrary.Models
{
    public class BookTitleTag
    {
        public int BookTitleId { get; set; }
        public BookTitle BookTitle { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
