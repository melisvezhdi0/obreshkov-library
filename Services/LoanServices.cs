using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Services
{
    public class LoanService
    {
        private readonly ObreshkovLibraryContext _db;

        public LoanService(ObreshkovLibraryContext db)
        {
            _db = db;
        }

        public async Task<int> LoanByTitleAsync(int clientId, int bookTitleId, int days = 14)
        {
            var availableCopy = await _db.BookCopies
                .Where(c => c.
               BookId == bookTitleId && c.IsActive)
                .Where(c => !_db.Loans.Any(l => l.BookCopyId == c.Id && l.ReturnDate == null))
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();

            if (availableCopy == null)
                throw new InvalidOperationException("Няма свободни копия от тази книга.");

            var loan = new Loan
            {
                ClientId = clientId,
                BookCopyId = availableCopy.Id,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(days)
            };

            _db.Loans.Add(loan);
            await _db.SaveChangesAsync();

            return loan.Id;
        }

        public async Task ReturnAsync(int loanId)
        {
            var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == loanId);
            if (loan == null)
                throw new InvalidOperationException("Това заемане не съществува.");

            if (loan.ReturnDate != null)
                throw new InvalidOperationException("Книгата вече е върната.");

            loan.ReturnDate = DateTime.Now;
            await _db.SaveChangesAsync();
        }
    }
}
