using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.Enums;

namespace ObreshkovLibrary.Services
{
    public interface IReaderNotificationService
    {
        Task CreateNotificationAsync(
            int readerId,
            string title,
            string message,
            ReaderNotificationType type,
            int? bookId = null,
            int? loanId = null,
            int? categoryId = null);

        Task ProcessLoanDueRemindersAsync();

        Task NotifyForNewCategoryAsync(Category category);
    }
}