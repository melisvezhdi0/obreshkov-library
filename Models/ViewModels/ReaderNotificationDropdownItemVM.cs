using ObreshkovLibrary.Models.Enums;

namespace ObreshkovLibrary.Models.ViewModels
{
    public class ReaderNotificationDropdownItemVM
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime CreatedOn { get; set; }
        public bool IsRead { get; set; }
        public ReaderNotificationType Type { get; set; }
    }
}