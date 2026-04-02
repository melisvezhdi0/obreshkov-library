using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoansController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly IStudentNotificationService _notificationService;

        public LoansController(
            ObreshkovLibraryContext context,
            IStudentNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

        [HttpGet]
        public async Task<IActionResult> SearchBook(string title, string? author, int clientId)
        {
            title = (title ?? string.Empty).Trim();
            author = string.IsNullOrWhiteSpace(author) ? null : author.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                return Json(new
                {
                    found = false,
                    message = "Въведи заглавие, за да потърсиш книга."
                });
            }

            var query = _context.Books
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(b => b.IsActive && b.Title.Contains(title));

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(b => b.Author.Contains(author));
            }

            var book = await query
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Author)
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return Json(new
                {
                    found = false,
                    message = "Не са намерени резултати."
                });
            }

            bool alreadyHasThisBook = await _context.Loans
                .Include(l => l.BookCopy)
                .AnyAsync(l =>
                    l.ClientId == clientId &&
                    l.ReturnDate == null &&
                    l.BookCopy != null &&
                    l.BookCopy.BookId == book.Id);

            if (alreadyHasThisBook)
            {
                return Json(new
                {
                    found = true,
                    alreadyTaken = true,
                    title = book.Title,
                    author = book.Author,
                    genre = book.Category?.ParentCategory?.Name ?? "Без жанр",
                    category = book.Category?.Name ?? "Без подкатегория",
                    coverPath = book.CoverPath,
                    availableCopies = 0,
                    hasAvailableCopy = false,
                    message = "Ученикът вече има заета тази книга."
                });
            }

            var availableCopiesCount = await _context.BookCopies
                .Where(c => c.BookId == book.Id && c.IsActive)
                .CountAsync(c => !_context.Loans.Any(l => l.BookCopyId == c.Id && l.ReturnDate == null));

            return Json(new
            {
                found = true,
                alreadyTaken = false,
                bookId = book.Id,
                title = book.Title,
                author = book.Author,
                genre = book.Category?.ParentCategory?.Name ?? "Без жанр",
                category = book.Category?.Name ?? "Без подкатегория",
                coverPath = book.CoverPath,
                availableCopies = availableCopiesCount,
                hasAvailableCopy = availableCopiesCount > 0,
                message = availableCopiesCount > 0
                    ? "Книгата е намерена и има свободно копие."
                    : "Няма свободни копия."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCreate(LoanCreateVM vm)
        {
            vm.Title = (vm.Title ?? string.Empty).Trim();
            vm.Author = string.IsNullOrWhiteSpace(vm.Author) ? null : vm.Author.Trim();

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == vm.ClientId);

            if (client == null)
                return NotFound();

            vm.ClientName = ($"{client.FirstName} {client.MiddleName} {client.LastName}")
                .Replace("  ", " ")
                .Trim();

            vm.CardNumber = client.CardNumber;

            if (vm.BookId == null || vm.BookId <= 0)
            {
                vm.ErrorMessage = "Моля, потърси книга и избери валиден резултат.";
                return View("Create", vm);
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == vm.BookId.Value && b.IsActive);

            if (book == null)
            {
                vm.ErrorMessage = "Избраната книга не беше намерена.";
                return View("Create", vm);
            }

            bool alreadyHasThisBook = await _context.Loans
                .Include(l => l.BookCopy)
                .AnyAsync(l =>
                    l.ClientId == client.Id &&
                    l.ReturnDate == null &&
                    l.BookCopy != null && l.BookCopy.BookId == book.Id);

            if (alreadyHasThisBook)
            {
                vm.ErrorMessage = "Ученикът вече има заета тази книга.";
                return View("Create", vm);
            }

            var availableCopy = await _context.BookCopies
                .Where(c => c.BookId == book.Id && c.IsActive)
                .Where(c => !_context.Loans
                    .Any(l => l.BookCopyId == c.Id && l.ReturnDate == null))
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();

            if (availableCopy == null)
            {
                vm.ErrorMessage = "Няма свободни копия от тази книга.";
                return View("Create", vm);
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

            return RedirectToAction("Details", "Clients", new { id = client.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
                return NotFound();

            if (loan.ReturnDate == null)
            {
                loan.ReturnDate = DateTime.Now;
                await _context.SaveChangesAsync();

                await _notificationService.ProcessAvailabilityNotificationsAsync();

                TempData["SuccessMessage"] = "Успешно върната книга.";
            }

            return RedirectToAction("Details", "Clients", new { id = loan.ClientId });
        }
    }
}