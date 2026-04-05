namespace ObreshkovLibrary.Models.ViewModels
{
    public class StudentLoanCardVM
    {
        public int BookId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public DateTime LoanDate { get; set; }

        public DateTime DueDate { get; set; }

        public bool IsOverdue { get; set; }

        public bool IsExtended { get; set; }
    }
}