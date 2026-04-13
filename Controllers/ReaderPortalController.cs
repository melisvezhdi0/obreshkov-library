using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.Enums;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Reader")]
    public class ReaderPortalController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ObreshkovLibraryContext _context;

        public ReaderPortalController(UserManager<IdentityUser> userManager, ObreshkovLibraryContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Challenge();
            }

            var currentLoans = await _context.Loans
                .Where(l => l.ReaderId == reader.Id && l.ReturnDate == null)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.LoanDate)
                .Take(5)
                .ToListAsync();

            var bookNotes = await _context.ReaderBookNotes
               .Where(n => n.ReaderId == reader.Id)
               .Include(n => n.Book)
               .OrderByDescending(n => n.UpdatedOn ?? n.CreatedOn)
               .ToListAsync();

            var vm = new ReaderDashboardVM
            {
                ReaderName = $"{reader.FirstName} {reader.LastName}".Trim(),
                CurrentLoansCount = await _context.Loans.CountAsync(l => l.ReaderId == reader.Id && l.ReturnDate == null),
                FavoritesCount = await _context.ReaderFavoriteBooks.CountAsync(f => f.ReaderId == reader.Id),
                UnreadNotificationsCount = await _context.ReaderNotifications.CountAsync(n => n.ReaderId == reader.Id && !n.IsRead),
                CurrentLoans = currentLoans.Select(l => new ReaderCurrentLoanVM
                {
                    BookId = l.BookCopy.Book.Id,
                    Title = l.BookCopy.Book.Title,
                    Author = l.BookCopy.Book.Author,
                    LoanDate = l.LoanDate,
                    DueDate = l.DueDate,
                    IsOverdue = l.DueDate.Date < DateTime.Today,
                    IsExtended = l.IsExtended
                }).ToList(),

                BookNotes = bookNotes.Select(n => new ReaderBookNoteVM
                {
                    NoteId = n.Id,
                    BookId = n.BookId,
                    BookTitle = n.Book?.Title ?? "Неизвестна книга",
                    BookAuthor = n.Book?.Author,
                    CoverPath = n.Book?.CoverPath,
                    Text = n.Text,
                    CreatedOn = n.CreatedOn,
                    UpdatedOn = n.UpdatedOn
                }).ToList()
            };

            ViewBag.RequirePasswordChange = !reader.PasswordChangedByReader;

            return View(vm);
        }

        public async Task<IActionResult> Favorites(string? sort)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Challenge();
            }

            var favorites = await _context.ReaderFavoriteBooks
                .Where(f => f.ReaderId == reader.Id)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                        .ThenInclude(c => c.ParentCategory)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Copies)
                .AsNoTracking()
                .ToListAsync();

            var copyIds = favorites
                .SelectMany(f => f.Book.Copies)
                .Where(c => c.IsActive)
                .Select(c => c.Id)
                .Distinct()
                .ToList();

            var activeLoanCopyIds = await _context.Loans
                .AsNoTracking()
                .Where(l => copyIds.Contains(l.BookCopyId) && l.ReturnDate == null)
                .Select(l => l.BookCopyId)
                .ToListAsync();

            var favoriteBooks = favorites
                .Select(f => new ReaderFavoriteBookVM
                {
                    BookId = f.BookId,
                    Title = f.Book.Title,
                    Author = f.Book.Author,
                    CoverPath = f.Book.CoverPath,
                    CategoryName = f.Book.Category != null
                        ? (f.Book.Category.ParentCategory != null
                            ? f.Book.Category.ParentCategory.Name + " / " + f.Book.Category.Name
                            : f.Book.Category.Name)
                        : "Без категория",
                    IsAvailable = f.Book.Copies.Any(c => c.IsActive && !activeLoanCopyIds.Contains(c.Id)),
                    CreatedOn = f.CreatedOn
                })
                .ToList();

            ViewBag.Sort = sort;

            return View(favoriteBooks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int bookId, string? returnUrl)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Challenge();
            }

            var existing = await _context.ReaderFavoriteBooks
                .FirstOrDefaultAsync(f => f.ReaderId == reader.Id && f.BookId == bookId);

            if (existing != null)
            {
                _context.ReaderFavoriteBooks.Remove(existing);
            }
            else
            {
                _context.ReaderFavoriteBooks.Add(new ReaderFavoriteBook
                {
                    ReaderId = reader.Id,
                    BookId = bookId,
                    CreatedOn = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("ReaderDetails", "Catalog", new { id = bookId });
        }

        public async Task<IActionResult> Notifications()
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Challenge();
            }

            var notifications = await _context.ReaderNotifications
                .Where(n => n.ReaderId == reader.Id)
                .OrderByDescending(n => n.CreatedOn)
                .Select(n => new ReaderNotificationItemVM
                {
                    NotificationId = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    CreatedOn = n.CreatedOn,
                    IsRead = n.IsRead,
                    BookId = n.BookId,
                    CategoryId = n.CategoryId,
                    Type = n.Type
                })
                .ToListAsync();

            ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationAsRead(int id, string? returnUrl = null)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Challenge();
            }

            var notification = await _context.ReaderNotifications
                .FirstOrDefaultAsync(n => n.Id == id && n.ReaderId == reader.Id);

            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Notifications));
        }

        [HttpGet]
        public async Task<IActionResult> GetNotesForBook(int bookId)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Unauthorized();
            }

            var notes = await _context.ReaderBookNotes
                .Where(n => n.ReaderId == reader.Id && n.BookId == bookId)
                .OrderByDescending(n => n.UpdatedOn ?? n.CreatedOn)
                .Select(n => new
                {
                    id = n.Id,
                    text = n.Text,
                    createdOn = n.CreatedOn.ToString("dd.MM.yyyy"),
                    updatedOn = n.UpdatedOn.HasValue ? n.UpdatedOn.Value.ToString("dd.MM.yyyy") : null
                })
                .ToListAsync();

            return Json(notes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNote(int bookId, string text)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("Бележката не може да бъде празна.");
            }

            var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
            if (!bookExists)
            {
                return NotFound();
            }

            var notesCount = await _context.ReaderBookNotes
            .CountAsync(n => n.ReaderId == reader.Id && n.BookId == bookId);

            if (notesCount >= 5)
            {
                return BadRequest("Можеш да имаш най-много 5 бележки към една книга.");
            }

            var note = new ReaderBookNote
            {
                ReaderId = reader.Id,
                BookId = bookId,
                Text = text.Trim(),
                CreatedOn = DateTime.Now
            };

            _context.ReaderBookNotes.Add(note);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                id = note.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNote(int noteId, string text)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("Бележката не може да бъде празна.");
            }

            var note = await _context.ReaderBookNotes
                .FirstOrDefaultAsync(n => n.Id == noteId && n.ReaderId == reader.Id);

            if (note == null)
            {
                return NotFound();
            }

            note.Text = text.Trim();
            note.UpdatedOn = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Unauthorized();
            }

            var note = await _context.ReaderBookNotes
                .FirstOrDefaultAsync(n => n.Id == noteId && n.ReaderId == reader.Id);

            if (note == null)
            {
                return NotFound();
            }

            _context.ReaderBookNotes.Remove(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private async Task<Reader?> GetCurrentReaderAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            var cardNumber = user.UserName?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return null;
            }

            return await _context.Readers
                .FirstOrDefaultAsync(r => r.CardNumber != null && r.CardNumber.ToUpper() == cardNumber);
        }
    }
}