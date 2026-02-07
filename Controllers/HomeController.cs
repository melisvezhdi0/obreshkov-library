using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ObreshkovLibrary.Models;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;


namespace ObreshkovLibrary.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index(string? cardNumber)
        {
            var vm = new HomeDashboardVM();

            if (string.IsNullOrWhiteSpace(cardNumber))
                return View(vm);

            cardNumber = cardNumber.Trim();
            vm.CardNumber = cardNumber;

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);

            if (client == null)
            {
                vm.ErrorMessage = "Няма читател с такъв номер на карта.";
                return View(vm);
            }

            vm.Client = client;

            vm.ActiveLoans = await _context.Loans
                .Where(l => l.ClientId == client.Id && l.ReturnDate == null)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.BookTitle)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchByCardNumber(string cardNumber)
        {
            return RedirectToAction(nameof(Index), new { cardNumber });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private readonly ILogger<HomeController> _logger;
        private readonly ObreshkovLibraryContext _context;

        public HomeController(ILogger<HomeController> logger, ObreshkovLibraryContext context)
        {
            _logger = logger;
            _context = context;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
