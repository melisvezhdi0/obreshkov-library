using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class StudentCurrentLoanVM
    {
        public int LoanId { get; set; }
        public int BookId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? CoverPath { get; set; }

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }

        public bool IsOverdue { get; set; }
        public bool IsExtended { get; set; }

        public string CurrentNote { get; set; } = string.Empty;

        public bool CanRequestExtension { get; set; }
        public bool HasExtensionRequest { get; set; }
        public LoanExtensionRequestStatus? ExtensionRequestStatus { get; set; }
    }
}