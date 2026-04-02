using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Services
{
    public interface IStudentNotificationService
    {
        Task CreateNotificationAsync(
            int clientId,
            string title,
            string message,
            StudentNotificationType type,
            int? bookId = null,
            int? loanId = null);

        Task ProcessLoanDueRemindersAsync();
        Task ProcessAvailabilityNotificationsAsync();
        Task NotifyForNewBookAsync(Book book);
        Task SendAdminNotificationToAllAsync(string title, string message);
    }
}