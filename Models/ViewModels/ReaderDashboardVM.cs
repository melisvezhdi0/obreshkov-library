namespace ObreshkovLibrary.Models.ViewModels
{
    public class ReaderDashboardVM
    {
        public string ReaderName { get; set; } = string.Empty;

        public int CurrentLoansCount { get; set; }
        public int FavoritesCount { get; set; }
        public int UnreadNotificationsCount { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? CardNumber { get; set; }
        public int? Grade { get; set; }
        public string? Section { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }

        public List<ReaderCurrentLoanVM> CurrentLoans { get; set; } = new();
        public List<ReaderFavoriteBookVM> FavoriteBooks { get; set; } = new();
        public List<ReaderNotificationItemVM> LatestNotifications { get; set; } = new();
        public List<ReaderLoanHistoryItemVM> LoanHistory { get; set; } = new();
        public List<ReaderBookNoteVM> BookNotes { get; set; } = new();
    }
}