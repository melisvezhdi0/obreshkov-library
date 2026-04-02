using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class StudentNotificationItemVM
    {
        public int NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public StudentNotificationType Type { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }

        public int? BookId { get; set; }
        public int? LoanId { get; set; }
    }
}