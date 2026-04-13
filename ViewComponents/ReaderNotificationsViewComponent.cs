using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.ViewComponents
{
    public class ReaderNotificationsViewComponent : ViewComponent
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReaderNotificationsViewComponent(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return View(new ReaderNotificationDropdownVM());
            }

            var cardNumber = user.UserName?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return View(new ReaderNotificationDropdownVM());
            }

            var reader = await _context.Readers
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.CardNumber != null && r.CardNumber.ToUpper() == cardNumber);

            if (reader == null)
            {
                return View(new ReaderNotificationDropdownVM());
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

            var vm = new ReaderNotificationDropdownVM
            {
                UnreadCount = unreadCount,
                Items = items
            };

            return View(vm);
        }
    }
}