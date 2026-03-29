using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Services
{
    public class StudentNotificationService : IStudentNotificationService
    {
        private readonly ObreshkovLibraryContext _context;

        public StudentNotificationService(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(
            int clientId,
            string title,
            string message,
            StudentNotificationType type,
            int? bookId = null,
            int? loanId = null)
        {
            _context.StudentNotifications.Add(new StudentNotification
            {
                ClientId = clientId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedOn = DateTime.Now,
                BookId = bookId,
                LoanId = loanId
            });

            await _context.SaveChangesAsync();
        }

        public async Task ProcessLoanDueRemindersAsync()
        {
            var today = DateTime.Today;

            var activeLoans = await _context.Loans
                .Include(l => l.BookCopy)
                    .ThenInclude(c => c.Book)
                .Include(l => l.Client)
                .Where(l => l.ReturnDate == null)
                .ToListAsync();

            foreach (var loan in activeLoans)
            {
                var daysLeft = (loan.DueDate.Date - today).Days;
                var title = $"Напомняне за книга: {loan.BookCopy.Book.Title}";

                if (daysLeft == 7 && !loan.Reminder7DaysSent)
                {
                    await CreateNotificationAsync(
                        loan.ClientId,
                        title,
                        $"Остават 7 дни до връщането на книгата „{loan.BookCopy.Book.Title}“. Датата за връщане е {loan.DueDate:dd.MM.yyyy}.",
                        StudentNotificationType.LoanReminder,
                        loan.BookCopy.BookId,
                        loan.Id);

                    loan.Reminder7DaysSent = true;
                }
                else if (daysLeft == 3 && !loan.Reminder3DaysSent)
                {
                    await CreateNotificationAsync(
                        loan.ClientId,
                        title,
                        $"Остават 3 дни до връщането на книгата „{loan.BookCopy.Book.Title}“. Датата за връщане е {loan.DueDate:dd.MM.yyyy}.",
                        StudentNotificationType.LoanReminder,
                        loan.BookCopy.BookId,
                        loan.Id);

                    loan.Reminder3DaysSent = true;
                }
                else if (daysLeft == 1 && !loan.Reminder1DaySent)
                {
                    await CreateNotificationAsync(
                        loan.ClientId,
                        title,
                        $"Книгата „{loan.BookCopy.Book.Title}“ трябва да бъде върната утре - {loan.DueDate:dd.MM.yyyy}.",
                        StudentNotificationType.LoanReminder,
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
                            loan.ClientId,
                            $"Просрочена книга: {loan.BookCopy.Book.Title}",
                            $"Книгата „{loan.BookCopy.Book.Title}“ е просрочена. Тя е трябвало да бъде върната на {loan.DueDate:dd.MM.yyyy}.",
                            StudentNotificationType.OverdueReminder,
                            loan.BookCopy.BookId,
                            loan.Id);

                        loan.LastOverdueReminderSentOn = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task ProcessAvailabilityNotificationsAsync()
        {
            var activeRequests = await _context.BookAvailabilityRequests
                .Include(r => r.Book)
                    .ThenInclude(b => b.Copies)
                .Include(r => r.Client)
                .Where(r => r.IsActive)
                .ToListAsync();

            foreach (var request in activeRequests)
            {
                var availableCopyIds = request.Book.Copies
                    .Where(c => c.IsActive)
                    .Select(c => c.Id)
                    .ToList();

                if (!availableCopyIds.Any())
                    continue;

                var activeLoanCopyIds = await _context.Loans
                    .Where(l => availableCopyIds.Contains(l.BookCopyId) && l.ReturnDate == null)
                    .Select(l => l.BookCopyId)
                    .ToListAsync();

                var isAvailable = request.Book.Copies
                    .Any(c => c.IsActive && !activeLoanCopyIds.Contains(c.Id));

                if (!isAvailable)
                    continue;

                await CreateNotificationAsync(
                    request.ClientId,
                    $"Книгата е налична: {request.Book.Title}",
                    $"Книгата „{request.Book.Title}“ вече е налична в библиотеката.",
                    StudentNotificationType.Availability,
                    request.BookId,
                    null);

                request.IsActive = false;
                request.NotifiedOn = DateTime.Now;
                request.DeactivatedOn = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        public async Task NotifyForNewBookAsync(Book book)
        {
            var clients = await _context.Clients
                .Include(c => c.FavoriteBooks)
                    .ThenInclude(f => f.Book)
                        .ThenInclude(b => b.Category)
                .ToListAsync();

            foreach (var client in clients)
            {
                var favoriteBooks = client.FavoriteBooks.Select(f => f.Book).ToList();

                var hasMatchingAuthor = favoriteBooks.Any(f =>
                    !string.IsNullOrWhiteSpace(f.Author) &&
                    !string.IsNullOrWhiteSpace(book.Author) &&
                    f.Author.Trim().ToLower() == book.Author.Trim().ToLower());

                var hasMatchingCategory = favoriteBooks.Any(f =>
                    f.CategoryId.HasValue &&
                    book.CategoryId.HasValue &&
                    f.CategoryId.Value == book.CategoryId.Value);

                if (!hasMatchingAuthor && !hasMatchingCategory)
                    continue;

                var alreadyHasNotification = await _context.StudentNotifications.AnyAsync(n =>
                    n.ClientId == client.Id &&
                    n.BookId == book.Id &&
                    n.Type == StudentNotificationType.NewFavoriteMatch);

                if (alreadyHasNotification)
                    continue;

                await CreateNotificationAsync(
                    client.Id,
                    $"Нова книга за теб: {book.Title}",
                    $"Добавена е нова книга, която съвпада с твоите интереси - „{book.Title}“ от {book.Author}.",
                    StudentNotificationType.NewFavoriteMatch,
                    book.Id,
                    null);
            }
        }

        public async Task SendAdminNotificationToAllAsync(string title, string message)
        {
            var clientIds = await _context.Clients
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var clientId in clientIds)
            {
                _context.StudentNotifications.Add(new StudentNotification
                {
                    ClientId = clientId,
                    Title = title,
                    Message = message,
                    Type = StudentNotificationType.Admin,
                    IsRead = false,
                    CreatedOn = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}