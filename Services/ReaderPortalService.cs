using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Services
{
    public class ReaderPortalService : IReaderPortalService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ObreshkovLibraryContext _context;

        public ReaderPortalService(UserManager<IdentityUser> userManager, ObreshkovLibraryContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Reader?> GetCurrentReaderAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                return null;
            }

            var cardNumber = user.UserName;
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                return null;
            }

            return await _context.Readers.FirstOrDefaultAsync(r => r.CardNumber == cardNumber && r.IsActive);
        }

        public async Task<ReaderDashboardVM?> BuildDashboardAsync(System.Security.Claims.ClaimsPrincipal userPrincipal)
        {
            var reader = await GetCurrentReaderAsync(userPrincipal);
            if (reader == null)
            {
                return null;
            }

            var currentLoans = await _context.Loans
                .AsNoTracking()
                .Where(l => l.ReaderId == reader.Id && l.ReturnDate == null)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            var favorites = await _context.ReaderFavoriteBooks
                .AsNoTracking()
                .Where(f => f.ReaderId == reader.Id)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Category)
                        .ThenInclude(c => c.ParentCategory)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Copies)
                .OrderByDescending(f => f.CreatedOn)
                .ToListAsync();

            var favoriteCopyIds = favorites
                .SelectMany(f => f.Book.Copies)
                .Where(c => c.IsActive)
                .Select(c => c.Id)
                .Distinct()
                .ToList();

            var activeLoanCopyIds = favoriteCopyIds.Count == 0
                ? new List<int>()
                : await _context.Loans
                    .AsNoTracking()
                    .Where(l => favoriteCopyIds.Contains(l.BookCopyId) && l.ReturnDate == null)
                    .Select(l => l.BookCopyId)
                    .ToListAsync();

            var notifications = await _context.ReaderNotifications
                .AsNoTracking()
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
                    LoanId = n.LoanId,
                    Type = n.Type
                })
                .ToListAsync();

            var history = await _context.Loans
                .AsNoTracking()
                .Where(l => l.ReaderId == reader.Id)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.LoanDate)
                .Select(l => new ReaderLoanHistoryItemVM
                {
                    LoanId = l.Id,
                    BookId = l.BookCopy.Book.Id,
                    Title = l.BookCopy.Book.Title,
                    Author = l.BookCopy.Book.Author,
                    CoverPath = l.BookCopy.Book.CoverPath,
                    LoanDate = l.LoanDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate
                })
                .ToListAsync();

            var bookNotes = await _context.ReaderBookNotes
                .AsNoTracking()
                .Where(n => n.ReaderId == reader.Id)
                .Include(n => n.Book)
                .OrderByDescending(n => n.UpdatedOn ?? n.CreatedOn)
                .ToListAsync();

            return new ReaderDashboardVM
            {
                ReaderName = $"{reader.FirstName} {reader.LastName}".Trim(),
                FirstName = reader.FirstName,
                MiddleName = reader.MiddleName,
                LastName = reader.LastName,
                PhoneNumber = reader.PhoneNumber,
                CardNumber = reader.CardNumber,
                Grade = reader.Grade,
                Section = reader.Section,
                IsActive = reader.IsActive,
                CreatedOn = reader.CreatedOn,

                CurrentLoansCount = currentLoans.Count,
                FavoritesCount = favorites.Count,
                UnreadNotificationsCount = notifications.Count(n => !n.IsRead),

                CurrentLoans = currentLoans.Select(l => new ReaderCurrentLoanVM
                {
                    LoanId = l.Id,
                    BookId = l.BookCopy.Book.Id,
                    Title = l.BookCopy.Book.Title,
                    Author = l.BookCopy.Book.Author,
                    CoverPath = l.BookCopy.Book.CoverPath,
                    LoanDate = l.LoanDate,
                    DueDate = l.DueDate,
                    IsOverdue = l.DueDate.Date < DateTime.Today,
                    IsExtended = l.IsExtended
                }).ToList(),

                FavoriteBooks = favorites.Select(f => new ReaderFavoriteBookVM
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
                }).ToList(),

                LatestNotifications = notifications,
                LoanHistory = history,

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
        }
    }
}