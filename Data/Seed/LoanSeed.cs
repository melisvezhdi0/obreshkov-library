using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data.Seed
{
    public static class LoanSeed
    {
        public static async Task SeedArchivedLoansAsync(ObreshkovLibraryContext context)
        {
            var existingSeedLoans = await context.Loans
                .IgnoreQueryFilters()
                .CountAsync(l => l.Notes != null && l.Notes.StartsWith("Архивен seed заем"));

            if (existingSeedLoans >= 6)
                return;

            var clients = await context.Clients
                .IgnoreQueryFilters()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Id)
                .Take(6)
                .ToListAsync();

            var copies = await context.BookCopies
                .IgnoreQueryFilters()
                .Include(c => c.Book)
                .Where(c => c.IsActive && c.Book.IsActive)
                .OrderBy(c => c.Id)
                .Take(20)
                .ToListAsync();

            if (clients.Count < 6 || copies.Count < 6)
                return;

            var availableCopies = new List<BookCopy>();

            foreach (var copy in copies)
            {
                bool hasOpenLoan = await context.Loans
                    .IgnoreQueryFilters()
                    .AnyAsync(l => l.BookCopyId == copy.Id && l.ReturnDate == null);

                if (!hasOpenLoan)
                    availableCopies.Add(copy);

                if (availableCopies.Count == 6)
                    break;
            }

            if (availableCopies.Count < 6)
                return;

            for (int i = existingSeedLoans; i < 6; i++)
            {
                context.Loans.Add(new Loan
                {
                    ClientId = clients[i].Id,
                    BookCopyId = availableCopies[i].Id,
                    LoanDate = DateTime.Now.AddDays(-(45 + i * 3)),
                    DueDate = DateTime.Now.AddDays(-(15 + i * 2)),
                    ReturnDate = DateTime.Now.AddDays(-(4 + i)),
                    Notes = $"Архивен seed заем {i + 1}",
                    IsExtended = i % 2 == 0
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
