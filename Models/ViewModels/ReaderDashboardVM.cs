namespace ObreshkovLibrary.Models.ViewModels
{
    public class ReaderDashboardVM
    {
        public string ReaderName { get; set; } = string.Empty;

        public int CurrentLoansCount { get; set; }
        public int FavoritesCount { get; set; }
        public int UnreadNotificationsCount { get; set; }

        public List<ReaderCurrentLoanVM> CurrentLoans { get; set; } = new();
        public List<ReaderFavoriteBookVM> FavoriteBooks { get; set; } = new();
        public List<ReaderNotificationItemVM> LatestNotifications { get; set; } = new();
    }
}