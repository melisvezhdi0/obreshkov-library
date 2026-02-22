using System.Collections.Generic;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class HomeDashboardVM
    {
        public string? CardNumber { get; set; }
        public string? ErrorMessage { get; set; }

        public List<Loan> LatestLoans { get; set; } = new();

        public int DueTodayCount { get; set; }
        public List<Loan> DueTodayLoans { get; set; } = new();

        public int OverdueCount { get; set; }
        public List<Loan> OverdueLoans { get; set; } = new();

        public List<Book> LatestBookTitles { get; set; } = new();
    }
}
