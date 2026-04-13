namespace ObreshkovLibrary.Models.ViewModels
{
    public class ReaderNotificationDropdownVM
    {
        public int UnreadCount { get; set; }

        public List<ReaderNotificationDropdownItemVM> Items { get; set; } = new();
    }
}