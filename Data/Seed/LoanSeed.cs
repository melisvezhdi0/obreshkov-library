using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data.Seed
{
    public static class LoanSeed
    {
        private const string ReturnedPrefix = "SeedLoan_Returned";
        private const string CurrentTwoPrefix = "SeedLoan_CurrentTwo";
        private const string CurrentOnePrefix = "SeedLoan_CurrentOne";
        private const string OverduePrefix = "SeedLoan_Overdue";
        private const string ArchivedPrefix = "ArchivedSeedLoan";

        public static async Task SeedLoansAsync(ObreshkovLibraryContext context)
        {
            var existingSeedLoansCount = await context.Loans
                .IgnoreQueryFilters()
                .CountAsync(l =>
                    l.Notes != null &&
                    (l.Notes.StartsWith(ReturnedPrefix) ||
                     l.Notes.StartsWith(CurrentTwoPrefix) ||
                     l.Notes.StartsWith(CurrentOnePrefix) ||
                     l.Notes.StartsWith(OverduePrefix)));

            if (existingSeedLoansCount > 0)
                return;

            var readers = await context.Readers
                .IgnoreQueryFilters()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Id)
                .ToListAsync();

            var copies = await context.BookCopies
                .IgnoreQueryFilters()
                .Include(c => c.Book)
                .Where(c => c.IsActive && c.Book.IsActive)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!readers.Any() || !copies.Any())
                return;

            var rng = new Random(42);

            var shuffledReaders = readers
                .OrderBy(_ => rng.Next())
                .ToList();

            int totalReaders = shuffledReaders.Count;

            int returnedReadersCount = Math.Max(1, (int)Math.Round(totalReaders * 0.70));
            int twoCurrentReadersCount = Math.Max(1, (int)Math.Round(totalReaders * 0.30));
            int oneCurrentReadersCount = Math.Max(1, (int)Math.Round(totalReaders * 0.40));
            int overdueReadersCount = Math.Max(1, (int)Math.Round(totalReaders * 0.25));

            var returnedReaders = shuffledReaders
                .Take(returnedReadersCount)
                .ToList();

            var twoCurrentReaders = shuffledReaders
                .Take(twoCurrentReadersCount)
                .ToList();

            var oneCurrentReaders = shuffledReaders
                .Skip(twoCurrentReadersCount)
                .Take(oneCurrentReadersCount)
                .ToList();

            var overdueReaders = shuffledReaders
                .Take(overdueReadersCount)
                .ToList();

            var openLoanCopyIds = await context.Loans
                .IgnoreQueryFilters()
                .Where(l => l.ReturnDate == null)
                .Select(l => l.BookCopyId)
                .ToListAsync();

            var availableActiveCopyIds = new HashSet<int>(
                copies
                    .Where(c => !openLoanCopyIds.Contains(c.Id))
                    .Select(c => c.Id));

            var loansToAdd = new List<Loan>();

            foreach (var reader in returnedReaders)
            {
                var returnedCopies = copies
                    .OrderBy(_ => rng.Next())
                    .Take(Math.Min(3, copies.Count))
                    .ToList();

                for (int i = 0; i < returnedCopies.Count; i++)
                {
                    var loanDate = DateTime.Today.AddDays(-(90 + rng.Next(10, 120)));
                    var dueDate = loanDate.AddDays(14);
                    var returnDate = dueDate.AddDays(rng.Next(0, 8));

                    loansToAdd.Add(new Loan
                    {
                        ReaderId = reader.Id,
                        BookCopyId = returnedCopies[i].Id,
                        LoanDate = loanDate,
                        DueDate = dueDate,
                        ReturnDate = returnDate,
                        Notes = $"{ReturnedPrefix}_{reader.Id}_{i + 1}",
                        IsExtended = false,
                        Reminder7DaysSent = false,
                        Reminder3DaysSent = false,
                        Reminder1DaySent = false,
                        LastOverdueReminderSentOn = null
                    });
                }
            }

            foreach (var reader in twoCurrentReaders)
            {
                var currentCopies = copies
                    .Where(c => availableActiveCopyIds.Contains(c.Id))
                    .OrderBy(_ => rng.Next())
                    .Take(2)
                    .ToList();

                int counter = 1;

                foreach (var copy in currentCopies)
                {
                    availableActiveCopyIds.Remove(copy.Id);

                    var loanDate = DateTime.Today.AddDays(-rng.Next(1, 10));
                    var dueOffsets = new[] { 7, 3, 1, 5, 9 };
                    var dueDate = DateTime.Today.AddDays(dueOffsets[rng.Next(dueOffsets.Length)]);

                    loansToAdd.Add(new Loan
                    {
                        ReaderId = reader.Id,
                        BookCopyId = copy.Id,
                        LoanDate = loanDate,
                        DueDate = dueDate,
                        ReturnDate = null,
                        Notes = $"{CurrentTwoPrefix}_{reader.Id}_{counter}",
                        IsExtended = false,
                        Reminder7DaysSent = false,
                        Reminder3DaysSent = false,
                        Reminder1DaySent = false,
                        LastOverdueReminderSentOn = null
                    });

                    counter++;
                }
            }

            foreach (var reader in oneCurrentReaders)
            {
                var copy = copies
                    .Where(c => availableActiveCopyIds.Contains(c.Id))
                    .OrderBy(_ => rng.Next())
                    .FirstOrDefault();

                if (copy == null)
                    break;

                availableActiveCopyIds.Remove(copy.Id);

                var loanDate = DateTime.Today.AddDays(-rng.Next(1, 10));
                var dueOffsets = new[] { 7, 3, 1, 6, 8 };
                var dueDate = DateTime.Today.AddDays(dueOffsets[rng.Next(dueOffsets.Length)]);

                loansToAdd.Add(new Loan
                {
                    ReaderId = reader.Id,
                    BookCopyId = copy.Id,
                    LoanDate = loanDate,
                    DueDate = dueDate,
                    ReturnDate = null,
                    Notes = $"{CurrentOnePrefix}_{reader.Id}_1",
                    IsExtended = false,
                    Reminder7DaysSent = false,
                    Reminder3DaysSent = false,
                    Reminder1DaySent = false,
                    LastOverdueReminderSentOn = null
                });
            }

            foreach (var reader in overdueReaders)
            {
                var copy = copies
                    .Where(c => availableActiveCopyIds.Contains(c.Id))
                    .OrderBy(_ => rng.Next())
                    .FirstOrDefault();

                if (copy == null)
                    break;

                availableActiveCopyIds.Remove(copy.Id);

                var loanDate = DateTime.Today.AddDays(-rng.Next(20, 40));
                var dueDate = DateTime.Today.AddDays(-rng.Next(4, 15));

                loansToAdd.Add(new Loan
                {
                    ReaderId = reader.Id,
                    BookCopyId = copy.Id,
                    LoanDate = loanDate,
                    DueDate = dueDate,
                    ReturnDate = null,
                    Notes = $"{OverduePrefix}_{reader.Id}_1",
                    IsExtended = false,
                    Reminder7DaysSent = false,
                    Reminder3DaysSent = false,
                    Reminder1DaySent = false,
                    LastOverdueReminderSentOn = null
                });
            }

            if (loansToAdd.Any())
            {
                context.Loans.AddRange(loansToAdd);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedArchivedLoansAsync(ObreshkovLibraryContext context)
        {
            var existingArchivedSeedLoans = await context.Loans
                .IgnoreQueryFilters()
                .CountAsync(l => l.Notes != null && l.Notes.StartsWith(ArchivedPrefix));

            if (existingArchivedSeedLoans >= 6)
                return;

            var readers = await context.Readers
                .IgnoreQueryFilters()
                .Where(r => r.IsActive)
                .OrderBy(r => r.Id)
                .Take(6)
                .ToListAsync();

            var copies = await context.BookCopies
                .IgnoreQueryFilters()
                .Include(c => c.Book)
                .Where(c => c.IsActive && c.Book.IsActive)
                .OrderBy(c => c.Id)
                .Take(20)
                .ToListAsync();

            if (readers.Count < 6 || copies.Count < 6)
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

            for (int i = existingArchivedSeedLoans; i < 6; i++)
            {
                context.Loans.Add(new Loan
                {
                    ReaderId = readers[i].Id,
                    BookCopyId = availableCopies[i].Id,
                    LoanDate = DateTime.Now.AddDays(-(45 + i * 3)),
                    DueDate = DateTime.Now.AddDays(-(15 + i * 2)),
                    ReturnDate = DateTime.Now.AddDays(-(4 + i)),
                    Notes = $"{ArchivedPrefix}_{i + 1}",
                    IsExtended = i % 2 == 0,
                    Reminder7DaysSent = false,
                    Reminder3DaysSent = false,
                    Reminder1DaySent = false,
                    LastOverdueReminderSentOn = null
                });
            }

            await context.SaveChangesAsync();
        }
    }
}