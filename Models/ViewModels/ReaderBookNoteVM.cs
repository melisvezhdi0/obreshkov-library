namespace ObreshkovLibrary.Models.ViewModels
{
    public class ReaderBookNoteVM
    {
        public int NoteId { get; set; }

        public int BookId { get; set; }

        public string BookTitle { get; set; } = string.Empty;

        public string? BookAuthor { get; set; }

        public string? CoverPath { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}