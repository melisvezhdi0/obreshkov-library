using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;

namespace ObreshkovLibrary.Services
{
    public class BookDeactivateService
    {
        private readonly ObreshkovLibraryContext _db;

        public BookDeactivateService(ObreshkovLibraryContext db)
        {
            _db = db;
        }

        public async Task DeactivateBookTitleAsync(int BookId)
        {
            bool hasActiveLoans = await _db.Loans
                .AnyAsync(l => l.ReturnDate == null &&
                               l.BookCopy.BookId == BookId);

            if (hasActiveLoans)
                throw new InvalidOperationException("Деактивирането е неуспешно, съществуват заети копия.");

            var bookTitle = await _db.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == BookId);

            if (bookTitle == null)
                throw new InvalidOperationException("Книгата не е намерена.");

            if (!bookTitle.IsActive)
                return;

            bookTitle.IsActive = false;

            var copies = await _db.BookCopies
                .IgnoreQueryFilters()
                .Where(c => c.BookId == BookId && c.IsActive)
                .ToListAsync();

            foreach (var copy in copies)
                copy.IsActive = false;

            await _db.SaveChangesAsync();
        }
        public async Task DeactivateBookCopyAsync(int bookCopyId)
        {
            bool isCurrentlyLoaned = await _db.Loans
                .AnyAsync(l => l.BookCopyId == bookCopyId && l.ReturnDate == null);

            if (isCurrentlyLoaned)
                throw new InvalidOperationException("Деактивирането на копието е неъспешно, книгата е заета.");

            var copy = await _db.BookCopies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == bookCopyId);

            if (copy == null)
                throw new InvalidOperationException("Копието не е намерено.");

            if (!copy.IsActive)
                return;

            copy.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
