using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;


namespace ObreshkovLibrary.Controllers
{
    public class BookTitlesController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public BookTitlesController(ObreshkovLibraryContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var books = await _context.BookTitles
                .Include(b => b.BookTitleCategories)
                    .ThenInclude(bc => bc.Category)
                .Include(b => b.BookCopies)
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new BookTitleCreateVM
            {
                Level1Options = await _context.Categories
                    .Where(c => c.ParentCategoryId == null && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookTitleCreateVM vm)
        {
            vm.Title = (vm.Title ?? "").Trim();
            vm.Author = (vm.Author ?? "").Trim();
            vm.CoverUrl = string.IsNullOrWhiteSpace(vm.CoverUrl) ? null : vm.CoverUrl.Trim();

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }

            var book = new BookTitle
            {
                Title = vm.Title,
                Author = vm.Author,
                Year = vm.Year,
                Description = vm.Description,
                CoverUrl = vm.CoverUrl,
                IsActive = true
            };

            _context.BookTitles.Add(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
