using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentPortalController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StudentPortalController(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var vm = new StudentDashboardVM
            {
                StudentName = $"{client.FirstName} {client.LastName}".Trim(),
                CurrentLoansCount = await _context.Loans
                    .AsNoTracking()
                    .CountAsync(l => l.ClientId == client.Id && l.ReturnDate == null),

                FavoritesCount = await _context.ClientFavoriteBooks
                    .AsNoTracking()
                    .CountAsync(f => f.ClientId == client.Id),

                UnreadNotificationsCount = await _context.StudentNotifications
                    .AsNoTracking()
                    .CountAsync(n => n.ClientId == client.Id && !n.IsRead),

                CurrentLoans = await GetCurrentLoansQuery(client.Id)
                    .OrderBy(l => l.DueDate)
                    .Take(5)
                    .ToListAsync(),

                FavoriteBooks = await GetFavoriteBooksQuery(client.Id)
                    .OrderByDescending(f => f.IsAvailable)
                    .ThenBy(f => f.Title)
                    .Take(6)
                    .ToListAsync(),

                LatestNotifications = await GetNotificationsQuery(client.Id)
                    .OrderBy(n => n.IsRead)
                    .ThenByDescending(n => n.CreatedOn)
                    .Take(6)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> MyBooks()
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var model = await GetCurrentLoansQuery(client.Id)
                .OrderBy(l => l.DueDate)
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var model = await _context.Loans
                .AsNoTracking()
                .Where(l => l.ClientId == client.Id && l.ReturnDate != null)
                .OrderByDescending(l => l.ReturnDate)
                .Select(l => new StudentLoanHistoryItemVM
                {
                    LoanId = l.Id,
                    BookId = l.BookCopy.BookId,
                    Title = l.BookCopy.Book.Title,
                    Author = l.BookCopy.Book.Author,
                    CoverPath = l.BookCopy.Book.CoverPath,
                    LoanDate = l.LoanDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate
                })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Favorites()
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var model = await GetFavoriteBooksQuery(client.Id)
                .OrderByDescending(f => f.IsAvailable)
                .ThenBy(f => f.Title)
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var model = await GetNotificationsQuery(client.Id)
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedOn)
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int bookId, string? returnUrl)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId && b.IsActive);
            if (!bookExists)
            {
                TempData["ErrorMessage"] = "Книгата не беше намерена.";
                return RedirectToLocalOrDefault(returnUrl, nameof(Favorites));
            }

            var favorite = await _context.ClientFavoriteBooks
                .FirstOrDefaultAsync(f => f.ClientId == client.Id && f.BookId == bookId);

            if (favorite == null)
            {
                _context.ClientFavoriteBooks.Add(new ClientFavoriteBook
                {
                    ClientId = client.Id,
                    BookId = bookId,
                    CreatedOn = DateTime.Now
                });

                TempData["SuccessMessage"] = "Книгата е добавена в любими.";
            }
            else
            {
                _context.ClientFavoriteBooks.Remove(favorite);
                TempData["SuccessMessage"] = "Книгата е премахната от любими.";
            }

            await _context.SaveChangesAsync();
            return RedirectToLocalOrDefault(returnUrl, nameof(Favorites));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAvailabilityNotification(int bookId, string? returnUrl)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var book = await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive);

            if (book == null)
            {
                TempData["ErrorMessage"] = "Книгата не беше намерена.";
                return RedirectToLocalOrDefault(returnUrl, nameof(Favorites));
            }

            var isAvailable = await _context.BookCopies
                .Where(c => c.BookId == bookId && c.IsActive)
                .AnyAsync(c => !_context.Loans.Any(l => l.BookCopyId == c.Id && l.ReturnDate == null));

            if (isAvailable)
            {
                TempData["InfoMessage"] = "Книгата вече е налична.";
                return RedirectToLocalOrDefault(returnUrl, nameof(Favorites));
            }

            var existingRequest = await _context.BookAvailabilityRequests
                .FirstOrDefaultAsync(r => r.ClientId == client.Id && r.BookId == bookId && r.IsActive);

            if (existingRequest != null)
            {
                TempData["InfoMessage"] = "Вече имаш активна заявка за уведомяване за тази книга.";
                return RedirectToLocalOrDefault(returnUrl, nameof(Favorites));
            }

            _context.BookAvailabilityRequests.Add(new BookAvailabilityRequest
            {
                ClientId = client.Id,
                BookId = bookId,
                IsActive = true,
                CreatedOn = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ще бъдеш уведомен, когато книгата стане налична.";
            return RedirectToLocalOrDefault(returnUrl, nameof(Favorites));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePersonalNote(int loanId, string text, string? returnUrl)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            text = (text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["ErrorMessage"] = "Бележката не може да е празна.";
                return RedirectToLocalOrDefault(returnUrl, nameof(MyBooks));
            }

            var loan = await _context.Loans
                .Include(l => l.PersonalNotes)
                .FirstOrDefaultAsync(l => l.Id == loanId && l.ClientId == client.Id && l.ReturnDate == null);

            if (loan == null)
            {
                TempData["ErrorMessage"] = "Заемането не беше намерено.";
                return RedirectToAction(nameof(MyBooks));
            }

            var existingNote = loan.PersonalNotes
                .OrderByDescending(n => n.Id)
                .FirstOrDefault();

            if (existingNote == null)
            {
                _context.LoanPersonalNotes.Add(new LoanPersonalNote
                {
                    LoanId = loan.Id,
                    Text = text,
                    CreatedOn = DateTime.Now
                });
            }
            else
            {
                existingNote.Text = text;
                existingNote.UpdatedOn = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Бележката е запазена.";
            return RedirectToLocalOrDefault(returnUrl, nameof(MyBooks));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestExtension(int loanId, int requestedDays, string? returnUrl)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            if (requestedDays != 3 && requestedDays != 5 && requestedDays != 7)
            {
                TempData["ErrorMessage"] = "Позволени са само 3, 5 или 7 дни.";
                return RedirectToLocalOrDefault(returnUrl, nameof(MyBooks));
            }

            var loan = await _context.Loans
                .Include(l => l.ExtensionRequests)
                .Include(l => l.BookCopy)
                    .ThenInclude(c => c.Book)
                .FirstOrDefaultAsync(l => l.Id == loanId && l.ClientId == client.Id && l.ReturnDate == null);

            if (loan == null)
            {
                TempData["ErrorMessage"] = "Заемането не беше намерено.";
                return RedirectToAction(nameof(MyBooks));
            }

            if (loan.DueDate.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Не можеш да подадеш заявка за удължаване за просрочена книга.";
                return RedirectToAction(nameof(MyBooks));
            }

            if (loan.IsExtended)
            {
                TempData["ErrorMessage"] = "Срокът за тази книга вече е бил удължен.";
                return RedirectToAction(nameof(MyBooks));
            }

            if (loan.ExtensionRequests.Any())
            {
                TempData["ErrorMessage"] = "За тази книга вече има подадена заявка за удължаване.";
                return RedirectToAction(nameof(MyBooks));
            }

            _context.LoanExtensionRequests.Add(new LoanExtensionRequest
            {
                LoanId = loan.Id,
                RequestedDays = requestedDays,
                Status = LoanExtensionRequestStatus.Pending,
                RequestedOn = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявката за удължаване е изпратена към библиотекаря.";
            return RedirectToLocalOrDefault(returnUrl, nameof(MyBooks));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int id, string? returnUrl)
        {
            var client = await GetCurrentClientAsync();
            if (client == null)
                return Challenge();

            var notification = await _context.StudentNotifications
                .FirstOrDefaultAsync(n => n.Id == id && n.ClientId == client.Id);

            if (notification == null)
            {
                TempData["ErrorMessage"] = "Съобщението не беше намерено.";
                return RedirectToAction(nameof(Notifications));
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToLocalOrDefault(returnUrl, nameof(Notifications));
        }

        private async Task<Client?> GetCurrentClientAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return null;

            var cardNumber = user.UserName?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(cardNumber))
                return null;

            return await _context.Clients
                .FirstOrDefaultAsync(c => c.CardNumber != null && c.CardNumber.ToUpper() == cardNumber);
        }

        private IQueryable<StudentCurrentLoanVM> GetCurrentLoansQuery(int clientId)
        {
            return _context.Loans
                .AsNoTracking()
                .Where(l => l.ClientId == clientId && l.ReturnDate == null)
                .Select(l => new StudentCurrentLoanVM
                {
                    LoanId = l.Id,
                    BookId = l.BookCopy.BookId,
                    Title = l.BookCopy.Book.Title,
                    Author = l.BookCopy.Book.Author,
                    CoverPath = l.BookCopy.Book.CoverPath,
                    LoanDate = l.LoanDate,
                    DueDate = l.DueDate,
                    IsOverdue = l.DueDate.Date < DateTime.Today,
                    IsExtended = l.IsExtended,
                    CurrentNote = l.PersonalNotes
                        .OrderByDescending(n => n.Id)
                        .Select(n => n.Text)
                        .FirstOrDefault() ?? string.Empty,
                    HasExtensionRequest = l.ExtensionRequests.Any(),
                    ExtensionRequestStatus = l.ExtensionRequests
                        .OrderByDescending(r => r.Id)
                        .Select(r => (LoanExtensionRequestStatus?)r.Status)
                        .FirstOrDefault(),
                    CanRequestExtension = !l.IsExtended
                        && !l.ExtensionRequests.Any()
                        && l.DueDate.Date >= DateTime.Today
                });
        }

        private IQueryable<StudentFavoriteBookVM> GetFavoriteBooksQuery(int clientId)
        {
            return _context.ClientFavoriteBooks
                .AsNoTracking()
                .Where(f => f.ClientId == clientId)
                .Select(f => new StudentFavoriteBookVM
                {
                    BookId = f.BookId,
                    Title = f.Book.Title,
                    Author = f.Book.Author,
                    CoverPath = f.Book.CoverPath,
                    CategoryName = f.Book.Category != null ? f.Book.Category.Name : "Без категория",
                    IsAvailable = f.Book.Copies
                        .Any(c => c.IsActive && !_context.Loans.Any(l => l.BookCopyId == c.Id && l.ReturnDate == null))
                });
        }

        private IQueryable<StudentNotificationItemVM> GetNotificationsQuery(int clientId)
        {
            return _context.StudentNotifications
                .AsNoTracking()
                .Where(n => n.ClientId == clientId)
                .Select(n => new StudentNotificationItemVM
                {
                    NotificationId = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedOn = n.CreatedOn,
                    BookId = n.BookId,
                    LoanId = n.LoanId
                });
        }

        private IActionResult RedirectToLocalOrDefault(string? returnUrl, string fallbackAction)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(fallbackAction);
        }
    }
}