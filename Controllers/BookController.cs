using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ObreshkovLibrary.Controllers
{
    public class BookController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly BookDeactivateService _bookDeactivate;

        public BookController(ObreshkovLibraryContext context, BookDeactivateService bookDeactivate)
        {
            _context = context;
            _bookDeactivate = bookDeactivate;
        }

        private static List<SelectListItem> BuildTagOptions()
        {
            return Enum.GetValues(typeof(BookTags))
                .Cast<BookTags>()
                .Where(t => t != BookTags.None)
                .Select(t => new SelectListItem
                {
                    Value = ((int)t).ToString(),
                    Text = TagToBg(t)
                })
                .ToList();
        }

        private static string TagToBg(BookTags t) => t switch
        {
            BookTags.Classic => "Класика",
            BookTags.Romance => "Любовен",
            BookTags.Drama => "Драма",
            BookTags.Fantasy => "Фентъзи",
            BookTags.Horror => "Ужаси",
            BookTags.Bulgarian => "Българска литература",
            BookTags.Foreign => "Чужда литература",
            BookTags.SchoolLiterature => "Училищна литература",
            _ => t.ToString()
        };

        private static BookTags BuildTagsFromSelected(List<int> selected)
        {
            if (selected == null || selected.Count == 0)
                return BookTags.None;

            BookTags tags = BookTags.None;

            foreach (var v in selected.Distinct())
                tags |= (BookTags)v;

            return tags;
        }

        private static List<int> TagsToSelectedValues(BookTags tags)
        {
            if (tags == BookTags.None) return new List<int>();

            return Enum.GetValues(typeof(BookTags))
                .Cast<BookTags>()
                .Where(t => t != BookTags.None && (tags & t) == t)
                .Select(t => (int)t)
                .Distinct()
                .ToList();
        }

        private static BookTags ParseTagsToEnum(string? tagsText)
        {
            if (string.IsNullOrWhiteSpace(tagsText)) return BookTags.None;

            BookTags result = BookTags.None;

            var tokens = tagsText
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => x.Length > 0)
                .Distinct()
                .Take(12);

            foreach (var t in tokens)
            {
                result |= t switch
                {
                    "classic" or "класика" => BookTags.Classic,
                    "romance" or "любовен" => BookTags.Romance,
                    "drama" or "драма" => BookTags.Drama,
                    "fantasy" or "фентъзи" => BookTags.Fantasy,
                    "horror" or "ужаси" => BookTags.Horror,
                    "bulgarian" or "българска" => BookTags.Bulgarian,
                    "foreign" or "чужда" => BookTags.Foreign,
                    "school" or "училищна" => BookTags.SchoolLiterature,
                    _ => BookTags.None
                };
            }

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b => b.IsActive)
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Author)
                .ToListAsync();

            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> Archived()
        {
            var books = await _context.Books
                .IgnoreQueryFilters()
                .Where(b => !b.IsActive)
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();

            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .IgnoreQueryFilters()
                .Include(b => b.Category)
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            var copyIds = book.Copies.Select(c => c.Id).ToList();

            var activeLoanCopyIds = await _context.Loans
                .Where(l => copyIds.Contains(l.BookCopyId) && l.ReturnDate == null)
                .Select(l => l.BookCopyId)
                .ToListAsync();

            ViewBag.ActiveLoanCopyIds = activeLoanCopyIds;

            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new BookCreateVM
            {
                Level1Options = await _context.Categories
                    .Where(c => c.ParentCategoryId == null && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),

                TagOptions = BuildTagOptions()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateVM vm)
        {
            vm.Title = (vm.Title ?? "").Trim();
            vm.Author = (vm.Author ?? "").Trim();

            var finalCategoryId = vm.Level2Id ?? vm.Level1Id;

            if (!finalCategoryId.HasValue)
                ModelState.AddModelError("", "Моля, изберете категория.");

            if (!ModelState.IsValid)
                return View(vm);

            using var tx = await _context.Database.BeginTransactionAsync();

            var book = new Book
            {
                Title = vm.Title,
                Author = vm.Author,
                Year = vm.Year,
                Description = vm.Description,
                CoverUrl = vm.CoverUrl,
                CategoryId = finalCategoryId.Value,
                Tags = BuildTagsFromSelected(vm.SelectedTagValues),
                IsActive = true
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            for (int i = 0; i < vm.CopiesCount; i++)
            {
                _context.BookCopies.Add(new BookCopy
                {
                    BookId = book.Id,
                    IsActive = true
                });
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _context.Books
                .IgnoreQueryFilters()
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            int? level1Id = null;
            int? level2Id = null;

            if (book.CategoryId.HasValue)
            {
                var cat = await _context.Categories
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == book.CategoryId.Value);

                if (cat != null)
                {
                    if (cat.ParentCategoryId.HasValue)
                    {
                        level1Id = cat.ParentCategoryId.Value;
                        level2Id = cat.Id;
                    }
                    else
                    {
                        level1Id = cat.Id;
                        level2Id = null;
                    }
                }
            }

            var vm = new BookCreateVM
            {
                Title = book.Title,
                Author = book.Author,
                Year = book.Year,
                Description = book.Description,
                CoverUrl = book.CoverUrl,
                Level1Id = level1Id,
                Level2Id = level2Id,

                CopiesCount = Math.Max(1, await _context.BookCopies
                    .IgnoreQueryFilters()
                    .CountAsync(c => c.BookId == book.Id)),

                Level1Options = await _context.Categories
                    .Where(c => c.ParentCategoryId == null && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),

                TagOptions = BuildTagOptions(),
                SelectedTagValues = TagsToSelectedValues(book.Tags)
            };

            ViewBag.BookId = book.Id;
            ViewBag.IsBookActive = book.IsActive;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookCreateVM vm)
        {
            var book = await _context.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            vm.Title = (vm.Title ?? "").Trim();
            vm.Author = (vm.Author ?? "").Trim();
            vm.CoverUrl = string.IsNullOrWhiteSpace(vm.CoverUrl) ? null : vm.CoverUrl.Trim();

            var finalCategoryId = vm.Level2Id ?? vm.Level1Id;
            if (!finalCategoryId.HasValue)
                ModelState.AddModelError("", "Моля, изберете категория.");

            if (!ModelState.IsValid)
            {
                vm.Level1Options = await _context.Categories
                    .Where(c => c.ParentCategoryId == null && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                vm.TagOptions = BuildTagOptions();
                ViewBag.BookId = book.Id;
                ViewBag.IsBookActive = book.IsActive;

                return View(vm);
            }

            book.Title = vm.Title;
            book.Author = vm.Author;
            book.Year = vm.Year;
            book.Description = vm.Description;
            book.CoverUrl = vm.CoverUrl;
            book.CategoryId = finalCategoryId.Value;
            book.Tags = BuildTagsFromSelected(vm.SelectedTagValues);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _bookDeactivate.DeactivateBookTitleAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateCopy(int id)
        {
            var copy = await _context.BookCopies
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (copy == null)
                return NotFound();

            bool hasActiveLoan = await _context.Loans
                .AnyAsync(l => l.BookCopyId == id && l.ReturnDate == null);

            if (hasActiveLoan)
            {
                TempData["Error"] = "Копието не може да бъде деактивирано, защото е заето.";
                return RedirectToAction(nameof(Details), new { id = copy.BookId });
            }

            copy.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = copy.BookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateCopy(int id)
        {
            var copy = await _context.BookCopies
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (copy == null)
                return NotFound();

            copy.IsActive = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = copy.BookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            var book = await _context.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            book.IsActive = true;

            var copies = await _context.BookCopies
                .IgnoreQueryFilters()
                .Where(c => c.BookId == id)
                .ToListAsync();

            foreach (var c in copies)
                c.IsActive = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Archived));
        }
    }
}