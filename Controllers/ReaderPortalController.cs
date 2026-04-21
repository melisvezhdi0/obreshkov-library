using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Reader")]
    public class ReaderPortalController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly IReaderPortalService _readerPortalService;

        public ReaderPortalController(
            ObreshkovLibraryContext context,
            IReaderPortalService readerPortalService)
        {
            _context = context;
            _readerPortalService = readerPortalService;
        }

        public async Task<IActionResult> Index()
        {
            var reader = await _readerPortalService.GetCurrentReaderAsync(User);
            if (reader == null)
            {
                return Challenge();
            }

            var vm = await _readerPortalService.BuildDashboardAsync(User);
            if (vm == null)
            {
                return Challenge();
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            var reader = await GetCurrentReaderAsync();
            if (reader == null)
            {
                return Unauthorized();
            }

            var unreadNotifications = await _context.ReaderNotifications
                .Where(n => n.ReaderId == reader.Id && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
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

            return RedirectToAction(nameof(Index));
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

            text = (text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest("Бележката не може да бъде празна.");
            }

            var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId && b.IsActive);
            if (!bookExists)
            {
                return NotFound("Книгата не беше намерена.");
            }

            var notesCount = await _context.ReaderBookNotes.CountAsync(n => n.ReaderId == reader.Id && n.BookId == bookId);
            if (notesCount >= 5)
            {
                return BadRequest("Можеш да имаш най-много 5 бележки към една книга.");
            }

            var note = new ReaderBookNote
            {
                ReaderId = reader.Id,
                BookId = bookId,
                Text = text,
                CreatedOn = DateTime.Now
            };

            _context.ReaderBookNotes.Add(note);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, id = note.Id });
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

            text = (text ?? string.Empty).Trim();
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

            note.Text = text;
            note.UpdatedOn = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
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

            return Ok(new { success = true });
        }

        private async Task<Reader?> GetCurrentReaderAsync()
        {
            return await _readerPortalService.GetCurrentReaderAsync(User);
        }
    }
}