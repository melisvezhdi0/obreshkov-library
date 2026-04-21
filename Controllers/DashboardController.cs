using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Services.Interfaces;
using System.Text.RegularExpressions;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            ObreshkovLibraryContext context,
            IDashboardService dashboardService)
        {
            _context = context;
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int latestLoansPage = 1)
        {
            var vm = await _dashboardService.BuildDashboardAsync(latestLoansPage);
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

            var readerId = await _dashboardService.FindReaderIdByCardNumberAsync(cardNumber);

            if (!readerId.HasValue)
            {
                TempData["HomeError"] = "Няма читател с такъв номер на карта.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Details", "Readers", new { id = readerId.Value });
        }
    }
}