namespace ObreshkovLibrary.Models.ViewModels
{
    public class StudentFavoriteBookVM
    {
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? CoverPath { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }
    }
}