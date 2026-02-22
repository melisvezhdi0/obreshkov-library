using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ObreshkovLibrary.Controllers
{
    public class LoansController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public LoansController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(int clientId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientId);
            if (client == null) return NotFound();

            var vm = new LoanCreateVM
            {
                ClientId = client.Id,
                ClientName = ($"{client.FirstName} {client.MiddleName} {client.LastName}").Replace("  ", " ").Trim(),
                CardNumber = client.CardNumber
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoanCreateVM vm)
        {
            vm.Title = (vm.Title ?? "").Trim();
            vm.Author = string.IsNullOrWhiteSpace(vm.Author) ? null : vm.Author.Trim();

            if (!ModelState.IsValid)
                return View(vm);

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == vm.ClientId);
            if (client == null) return NotFound();

            vm.ClientName = ($"{client.FirstName} {client.MiddleName} {client.LastName}").Replace("  ", " ").Trim();
            vm.CardNumber = client.CardNumber;

            var query = _context.Books.AsQueryable();
            query = query.Where(b => b.Title.Contains(vm.Title));

            if (vm.Author != null)
                query = query.Where(b => b.Author.Contains(vm.Author));

            var book = await query.OrderBy(b => b.Title).FirstOrDefaultAsync();

            if (book == null)
            {
                vm.ErrorMessage = "Няма намерена книга по тези данни.";
                return View(vm);
            }

            var availableCopy = await _context.BookCopies
                .Where(c => c.BookId == book.Id && c.IsActive)
                .Where(c => !_context.Loans.Any(l => l.BookCopyId == c.Id && l.ReturnDate == null))
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();

            if (availableCopy == null)
            {
                vm.ErrorMessage = "Няма свободни копия от тази книга.";
                return View(vm);
            }

            var loan = new Loan
            {
                ClientId = client.Id,
                BookCopyId = availableCopy.Id,
                LoanDate = DateTime.Now,
                ReturnDate = null
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home", new { cardNumber = client.CardNumber });
        }
    }
}