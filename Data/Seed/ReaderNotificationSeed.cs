using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.Enums;

namespace ObreshkovLibrary.Data.Seed
{
    public static class ReaderNotificationSeed
    {
        public static async Task SeedReaderNotificationsAsync(ObreshkovLibraryContext context)
        {
            var existingSeedNotifications = await context.ReaderNotifications
                .IgnoreQueryFilters()
                .CountAsync(n => n.Message.StartsWith("Seed notification:"));

            if (existingSeedNotifications > 0)
                return;

            var readers = await context.Readers
                .IgnoreQueryFilters()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Id)
                .ToListAsync();

            if (!readers.Any())
                return;

            var loans = await context.Loans
                .IgnoreQueryFilters()
                .Include(l => l.BookCopy)
                    .ThenInclude(c => c.Book)
                .Where(l => l.ReturnDate == null)
                .OrderByDescending(l => l.Id)
                .ToListAsync();

            var categories = await context.Categories
                .IgnoreQueryFilters()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Id)
                .ToListAsync();

            var notifications = new List<ReaderNotification>();
            var rng = new Random(42);

            foreach (var reader in readers.Take(Math.Max(3, readers.Count)))
            {
                var readerLoans = loans.Where(l => l.ReaderId == reader.Id).ToList();

                if (readerLoans.Any())
                {
                    var normalLoan = readerLoans.FirstOrDefault(l => l.DueDate.Date >= DateTime.Today);
                    if (normalLoan != null)
                    {
                        notifications.Add(new ReaderNotification
                        {
                            ReaderId = reader.Id,
                            Title = $"Напомняне за връщане: {normalLoan.BookCopy.Book.Title}",
                            Message = $"Seed notification: Книгата „{normalLoan.BookCopy.Book.Title}“ трябва да се върне скоро.",
                            CreatedOn = DateTime.Now.AddDays(-rng.Next(0, 3)).AddHours(-rng.Next(1, 10)),
                            IsRead = rng.Next(0, 2) == 0,
                            Type = ReaderNotificationType.LoanReminder,
                            LoanId = normalLoan.Id,
                            BookId = normalLoan.BookCopy.BookId
                        });
                    }

                    var overdueLoan = readerLoans.FirstOrDefault(l => l.DueDate.Date < DateTime.Today);
                    if (overdueLoan != null)
                    {
                        notifications.Add(new ReaderNotification
                        {
                            ReaderId = reader.Id,
                            Title = $"Просрочена книга: {overdueLoan.BookCopy.Book.Title}",
                            Message = $"Seed notification: Книгата „{overdueLoan.BookCopy.Book.Title}“ е просрочена.",
                            CreatedOn = DateTime.Now.AddDays(-rng.Next(0, 5)).AddHours(-rng.Next(1, 10)),
                            IsRead = false,
                            Type = ReaderNotificationType.OverdueReminder,
                            LoanId = overdueLoan.Id,
                            BookId = overdueLoan.BookCopy.BookId
                        });
                    }
                }

                notifications.Add(new ReaderNotification
                {
                    ReaderId = reader.Id,
                    Title = "Важно съобщение от библиотеката",
                    Message = "Seed notification: Библиотеката ще работи с променено работно време тази седмица.",
                    CreatedOn = DateTime.Now.AddDays(-rng.Next(1, 7)).AddHours(-rng.Next(1, 10)),
                    IsRead = rng.Next(0, 2) == 0,
                    Type = ReaderNotificationType.Admin
                });

                var category = categories.OrderBy(_ => rng.Next()).FirstOrDefault();
                if (category != null)
                {
                    notifications.Add(new ReaderNotification
                    {
                        ReaderId = reader.Id,
                        Title = $"Нова категория: {category.Name}",
                        Message = $"Seed notification: Добавена е нова категория: {category.Name}.",
                        CreatedOn = DateTime.Now.AddDays(-rng.Next(1, 8)).AddHours(-rng.Next(1, 10)),
                        IsRead = rng.Next(0, 2) == 0,
                        Type = ReaderNotificationType.NewCategory,
                        CategoryId = category.Id
                    });
                }
            }

            if (notifications.Any())
            {
                context.ReaderNotifications.AddRange(notifications);
                await context.SaveChangesAsync();
            }
        }
    }
}