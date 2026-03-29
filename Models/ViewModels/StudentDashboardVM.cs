namespace ObreshkovLibrary.Models.ViewModels
{
    public class StudentDashboardVM
    {
        public string StudentName { get; set; } = string.Empty;

        public int CurrentLoansCount { get; set; }
        public int FavoritesCount { get; set; }
        public int UnreadNotificationsCount { get; set; }

        public List<StudentCurrentLoanVM> CurrentLoans { get; set; } = new();
        public List<StudentFavoriteBookVM> FavoriteBooks { get; set; } = new();
        public List<StudentNotificationItemVM> LatestNotifications { get; set; } = new();
    }
}