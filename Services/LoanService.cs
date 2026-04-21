using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ObreshkovLibrary.Services
{
    public class LoanService : ILoanService
    {
        private readonly ObreshkovLibraryContext _context;

        public LoanService(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateLoanAsync(int readerId, int bookId)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive);

            if (book == null)
                return false;

            bool alreadyHasThisBook = await _context.Loans
                .Include(l => l.BookCopy)
                .AnyAsync(l =>
                    l.ReaderId == readerId &&
                    l.ReturnDate == null &&
                    l.BookCopy != null &&
                    l.BookCopy.BookId == book.Id);

            if (alreadyHasThisBook)
                return false;

            var availableCopy = await _context.BookCopies
                .Where(c => c.BookId == book.Id && c.IsActive)
                .Where(c => !_context.Loans.Any(l => l.BookCopyId == c.Id && l.ReturnDate == null))
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();

            if (availableCopy == null)
                return false;

            var loan = new Loan
            {
                ReaderId = readerId,
                BookCopyId = availableCopy.Id,
                LoanDate = DateTime.Now,
                ReturnDate = null
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReturnLoanAsync(int loanId)
        {
            var loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null)
                return false;

            if (loan.ReturnDate != null)
                return false;

            loan.ReturnDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}