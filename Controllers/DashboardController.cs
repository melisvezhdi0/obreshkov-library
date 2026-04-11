using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;
using System.Text.RegularExpressions;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public DashboardController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int latestLoansPage = 1)
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
                .Include(l => l.reader)
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
                .Include(l => l.reader)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.DueTodayCount = vm.DueTodayLoans.Count;

            vm.OverdueLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate.Date < today)
                .Include(l => l.reader)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.OverdueCount = vm.OverdueLoans.Count;

            vm.OpenPasswordResetRequests = await _context.PasswordResetRequests
                .Where(r => !r.IsCompleted)
                .Include(r => r.reader)
                .OrderByDescending(r => r.RequestedOn)
                .Take(50)
                .ToListAsync();
            vm.OpenPasswordResetRequestsCount = vm.OpenPasswordResetRequests.Count;

            vm.LatestBookTitles = await _context.Books
                .OrderByDescending(b => b.CreatedOn)
                .ThenByDescending(b => b.Id)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }

        public IActionResult Details(int id)
        {
            var book = _context.Books
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchByCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                TempData["HomeError"] = "Моля, въведи номер на карта.";
                return RedirectToAction(nameof(Index));
            }

            cardNumber = cardNumber.Trim();

            if (!Regex.IsMatch(cardNumber, @"^\d{6}$"))
            {
                TempData["HomeError"] = "Номерът на читателската карта трябва да бъде точно 6 цифри.";
                return RedirectToAction(nameof(Index));
            }

            var reader = await _context.readers
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

            if (reader == null)
            {
                TempData["HomeError"] = "Няма читател с такъв номер на карта.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Details", "readers", new { id = reader.Id });
        }
    }
}