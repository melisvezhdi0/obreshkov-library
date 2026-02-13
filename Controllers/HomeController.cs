using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public HomeController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var vm = new HomeDashboardVM();

            // Последни заемания (8)
            vm.LatestLoans = await _context.Loans
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.BookTitle)
                .OrderByDescending(l => l.LoanDate)
                .Take(8)
                .ToListAsync();

            // За връщане днес (активни)
            vm.DueTodayLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate.Date == today)
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.BookTitle)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.DueTodayCount = vm.DueTodayLoans.Count;

            // Просрочени (активни)
            vm.OverdueLoans = await _context.Loans
                .Where(l => l.ReturnDate == null && l.DueDate.Date < today)
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.BookTitle)
                .OrderBy(l => l.DueDate)
                .Take(50)
                .ToListAsync();
            vm.OverdueCount = vm.OverdueLoans.Count;

            // Последно добавени книги
            vm.LatestBookTitles = await _context.BookTitles
                .OrderByDescending(b => b.Id)
                .Take(4)
                .ToListAsync();

            return View(vm);
        }
        public IActionResult Details(int id)
        {
            var book = _context.BookTitles
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
