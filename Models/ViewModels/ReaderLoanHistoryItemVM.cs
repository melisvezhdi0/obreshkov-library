namespace ObreshkovLibrary.Models.ViewModels
{
    public class ReaderLoanHistoryItemVM
    {
        public int LoanId { get; set; }
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? CoverPath { get; set; }

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}