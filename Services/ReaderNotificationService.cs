using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.Enums;

namespace ObreshkovLibrary.Services
{
    public class ReaderNotificationService : IReaderNotificationService
    {
        private readonly ObreshkovLibraryContext _context;

        public ReaderNotificationService(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        public Task CreateNotificationAsync(
            int readerId,
            string title,
            string message,
            ReaderNotificationType type,
            int? bookId = null,
            int? loanId = null,
            int? categoryId = null)
        {
            _context.ReaderNotifications.Add(new ReaderNotification
            {
                ReaderId = readerId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedOn = DateTime.Now,
                BookId = bookId,
                LoanId = loanId,
                CategoryId = categoryId
            });

            return Task.CompletedTask;
        }

        public async Task ProcessLoanDueRemindersAsync()
        {
            var today = DateTime.Today;

            var activeLoans = await _context.Loans
                .Include(l => l.BookCopy)
                    .ThenInclude(c => c.Book)
                .Include(l => l.Reader)
                .Where(l => l.ReturnDate == null)
                .ToListAsync();

            foreach (var loan in activeLoans)
            {
                var daysLeft = (loan.DueDate.Date - today).Days;
                var bookTitle = loan.BookCopy.Book.Title;

                if (daysLeft == 7 && !loan.Reminder7DaysSent)
                {
                    await CreateNotificationAsync(
                        loan.ReaderId,
                        $"Напомняне за връщане: {bookTitle}",
                        $"Книгата „{bookTitle}“ трябва да се върне до 7 дни.",
                        ReaderNotificationType.LoanReminder,
                        loan.BookCopy.BookId,
                        loan.Id);

                    loan.Reminder7DaysSent = true;
                }
                else if (daysLeft == 3 && !loan.Reminder3DaysSent)
                {
                    await CreateNotificationAsync(
                        loan.ReaderId,
                        $"Напомняне за връщане: {bookTitle}",
                        $"Книгата „{bookTitle}“ трябва да се върне до 3 дни.",
                        ReaderNotificationType.LoanReminder,
                        loan.BookCopy.BookId,
                        loan.Id);

                    loan.Reminder3DaysSent = true;
                }
                else if (daysLeft == 1 && !loan.Reminder1DaySent)
                {
                    await CreateNotificationAsync(
                        loan.ReaderId,
                        $"Напомняне за връщане: {bookTitle}",
                        $"Книгата „{bookTitle}“ трябва да се върне най-късно утре.",
                        ReaderNotificationType.LoanReminder,
                        loan.BookCopy.BookId,
                        loan.Id);

                    loan.Reminder1DaySent = true;
                }
                else if (daysLeft < 0)
                {
                    var shouldSendOverdueReminder =
                        loan.LastOverdueReminderSentOn == null ||
                        (today - loan.LastOverdueReminderSentOn.Value.Date).Days >= 3;

                    if (shouldSendOverdueReminder)
                    {
                        await CreateNotificationAsync(
                            loan.ReaderId,
                            $"Просрочена книга: {bookTitle}",
                            $"Книгата „{bookTitle}“ е просрочена.",
                            ReaderNotificationType.OverdueReminder,
                            loan.BookCopy.BookId,
                            loan.Id);

                        loan.LastOverdueReminderSentOn = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task NotifyForNewCategoryAsync(Category category)
        {
            var readerIds = await _context.Readers
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var readerId in readerIds)
            {
                var alreadyHasNotification = await _context.ReaderNotifications.AnyAsync(n =>
                    n.ReaderId == readerId &&
                    n.CategoryId == category.Id &&
                    n.Type == ReaderNotificationType.NewCategory);

                if (alreadyHasNotification)
                    continue;

                await CreateNotificationAsync(
                    readerId,
                    $"Нова категория: {category.Name}",
                    $"Добавена е нова категория: {category.Name}.",
                    ReaderNotificationType.NewCategory,
                    null,
                    null,
                    category.Id);
            }

            await _context.SaveChangesAsync();
        }
    }
}