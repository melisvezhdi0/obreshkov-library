using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services.Interfaces;
using System.Security.Claims;

namespace ObreshkovLibrary.Services
{
    public class ReaderNotificationService : IReaderNotificationService
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReaderNotificationService(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task ProcessLoanDueRemindersAsync()
        {
            var today = DateTime.Today;

            var loans = await _context.Loans
                .Include(l => l.Reader)
                .Include(l => l.BookCopy)
                    .ThenInclude(c => c.Book)
                .Where(l =>
                    l.ReturnDate == null &&
                    l.DueDate.Date <= today)
                .ToListAsync();

            foreach (var loan in loans)
            {
                var alreadyExists = await _context.ReaderNotifications
                    .AnyAsync(n =>
                        n.ReaderId == loan.ReaderId &&
                        n.LoanId == loan.Id &&
                        n.Type == Models.Enums.ReaderNotificationType.OverdueReminder);

                if (alreadyExists)
                    continue;

                _context.ReaderNotifications.Add(new Models.ReaderNotification
                {
                    ReaderId = loan.ReaderId,
                    LoanId = loan.Id,
                    Title = "Просрочен заем",
                    Message = $"Книгата \"{loan.BookCopy.Book.Title}\" е просрочена.",
                    CreatedOn = DateTime.Now,
                    Type = Models.Enums.ReaderNotificationType.OverdueReminder,
                    IsRead = false
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ReaderNotificationDropdownVM> BuildDropdownAsync(ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);

            if (user == null)
            {
                return new ReaderNotificationDropdownVM();
            }

            var cardNumber = user.UserName?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return new ReaderNotificationDropdownVM();
            }

            var reader = await _context.Readers
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.CardNumber != null && r.CardNumber.ToUpper() == cardNumber);

            if (reader == null)
            {
                return new ReaderNotificationDropdownVM();
            }

            var unreadCount = await _context.ReaderNotifications
                .AsNoTracking()
                .CountAsync(n => n.ReaderId == reader.Id && !n.IsRead);

            var items = await _context.ReaderNotifications
                .AsNoTracking()
                .Where(n => n.ReaderId == reader.Id)
                .OrderByDescending(n => n.CreatedOn)
                .Select(n => new ReaderNotificationDropdownItemVM
                {
                    NotificationId = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    CreatedOn = n.CreatedOn,
                    IsRead = n.IsRead,
                    CategoryId = n.CategoryId,
                    Type = n.Type
                })
                .ToListAsync();

            return new ReaderNotificationDropdownVM
            {
                UnreadCount = unreadCount,
                Items = items
            };
        }
    }
}