using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;

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
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var vm = new HomeDashboardVM();

            vm.LatestLoans = await _context.Loans
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.LoanDate)
                .Take(8)
                .ToListAsync();

            vm.DueTodayLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate.Date == today)
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.DueTodayCount = vm.DueTodayLoans.Count;

            vm.OverdueLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate.Date < today)
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.OverdueCount = vm.OverdueLoans.Count;

            vm.OpenPasswordResetRequests = await _context.PasswordResetRequests
                .Where(r => !r.IsCompleted)
                .Include(r => r.Client)
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
                TempData["HomeError"] = "Моля въведи номер на карта.";
                return RedirectToAction(nameof(Index));
            }

            cardNumber = cardNumber.Trim();

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

            if (client == null)
            {
                TempData["HomeError"] = "Няма читател с такъв номер на карта.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Details", "Clients", new { id = client.Id });
        }
    }
}