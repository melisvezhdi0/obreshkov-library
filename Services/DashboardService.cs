using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ObreshkovLibraryContext _context;

        public DashboardService(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        public async Task<HomeDashboardVM> BuildDashboardAsync(int latestLoansPage)
        {
            var today = DateTime.Today;
            var latestLoansPageSize = 4;
            var latestLoansStartDate = today.AddDays(-2);

            if (latestLoansPage < 1)
            {
                latestLoansPage = 1;
            }

            var latestLoansQuery = _context.Loans
                .Where(l => l.LoanDate.Date >= latestLoansStartDate && l.LoanDate.Date <= today)
                .Include(l => l.Reader)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.LoanDate)
                .ThenByDescending(l => l.Id);

            var latestLoansTotalCount = await latestLoansQuery.CountAsync();
            var latestLoansTotalPages = latestLoansTotalCount == 0
                ? 1
                : (int)Math.Ceiling(latestLoansTotalCount / (double)latestLoansPageSize);

            if (latestLoansPage > latestLoansTotalPages)
            {
                latestLoansPage = latestLoansTotalPages;
            }

            var vm = new HomeDashboardVM
            {
                LatestLoansCurrentPage = latestLoansPage,
                LatestLoansPageSize = latestLoansPageSize,
                LatestLoansTotalCount = latestLoansTotalCount,
                LatestLoansTotalPages = latestLoansTotalPages,
                LatestLoans = await latestLoansQuery
                    .Skip((latestLoansPage - 1) * latestLoansPageSize)
                    .Take(latestLoansPageSize)
                    .ToListAsync()
            };

            vm.DueTodayLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate.Date == today)
                .Include(l => l.Reader)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.DueTodayCount = vm.DueTodayLoans.Count;

            vm.OverdueLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate.Date < today)
                .Include(l => l.Reader)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.OverdueCount = vm.OverdueLoans.Count;

            vm.OpenPasswordResetRequests = await _context.PasswordResetRequests
                .Where(r => !r.IsCompleted)
                .Include(r => r.Reader)
                .OrderByDescending(r => r.RequestedOn)
                .Take(50)
                .ToListAsync();
            vm.OpenPasswordResetRequestsCount = vm.OpenPasswordResetRequests.Count;

            vm.LatestBookTitles = await _context.Books
                .OrderByDescending(b => b.CreatedOn)
                .ThenByDescending(b => b.Id)
                .Take(5)
                .ToListAsync();

            return vm;
        }

        public async Task<int?> FindReaderIdByCardNumberAsync(string cardNumber)
        {
            var reader = await _context.Readers
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

            return reader?.Id;
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}