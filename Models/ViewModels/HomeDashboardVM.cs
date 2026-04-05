using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class HomeDashboardVM
    {
        public List<Loan> LatestLoans { get; set; } = new();
        public int LatestLoansTotalCount { get; set; }
        public int LatestLoansCurrentPage { get; set; } = 1;
        public int LatestLoansPageSize { get; set; } = 4;
        public int LatestLoansTotalPages { get; set; }

        public List<Loan> DueTodayLoans { get; set; } = new();
        public int DueTodayCount { get; set; }

        public List<Loan> OverdueLoans { get; set; } = new();
        public int OverdueCount { get; set; }

        public List<PasswordResetRequest> OpenPasswordResetRequests { get; set; } = new();
        public int OpenPasswordResetRequestsCount { get; set; }

        public List<Book> LatestBookTitles { get; set; } = new();
    }
}