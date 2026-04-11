using ObreshkovLibrary.Models.Enums;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class ReaderNotificationItemVM
    {
        public int NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public ReaderNotificationType Type { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? CategoryId { get; set; }
        public int? BookId { get; set; }
        public int? LoanId { get; set; }
    }
}