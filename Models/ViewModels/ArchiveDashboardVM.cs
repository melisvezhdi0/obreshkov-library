using System.Collections.Generic;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class ArchiveDashboardVM
    {
        public string Search { get; set; } = string.Empty;

        public int BooksCount { get; set; }
        public int ClientsCount { get; set; }
        public int CategoriesCount { get; set; }
        public int CopiesCount { get; set; }
        public int LoansCount { get; set; }
        public int SchoolNewsCount { get; set; }
        public int PasswordRequestsCount { get; set; }

        public List<Book> RecentBooks { get; set; } = new();
        public List<Client> RecentClients { get; set; } = new();
        public List<Category> RecentCategories { get; set; } = new();
        public List<BookCopy> RecentCopies { get; set; } = new();
        public List<Loan> RecentLoans { get; set; } = new();
        public List<SchoolNews> RecentSchoolNews { get; set; } = new();
        public List<PasswordResetRequest> RecentPasswordRequests { get; set; } = new();
    }
}