using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ObreshkovLibrary.Controllers
{
    public class BookController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public BookController(ObreshkovLibraryContext context)
        {
            _context = context;
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
            {
                tags |= (BookTags)v;
            }

            return tags;
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
                    "romance" or "любовен" or "любовна" or "любовен роман" => BookTags.Romance,
                    "drama" or "драма" => BookTags.Drama,
                    "fantasy" or "фентъзи" => BookTags.Fantasy,
                    "horror" or "ужас" or "ужаси" => BookTags.Horror,
                    "bulgarian" or "българска" or "българска литература" => BookTags.Bulgarian,
                    "foreign" or "чужда" or "чужда литература" => BookTags.Foreign,
                    "school" or "училищна" or "училищна литература" => BookTags.SchoolLiterature,
                    _ => BookTags.None
                };
            }

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? title, string? author, int? year, string? tag, string? categoryIds)
        {
            title = (title ?? "").Trim();
            author = (author ?? "").Trim();
            tag = (tag ?? "").Trim();

            var selectedCategoryIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(categoryIds))
            {
                selectedCategoryIds = categoryIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .Distinct()
                    .Take(50)
                    .ToList();
            }

            bool hasTitle = !string.IsNullOrWhiteSpace(title);
            bool hasAuthor = !string.IsNullOrWhiteSpace(author);
            bool hasYear = year.HasValue;

            var tagEnum = ParseTagsToEnum(tag);
            bool hasTag = tagEnum != BookTags.None;

            bool hasCats = selectedCategoryIds.Count > 0;

            var q = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                .AsQueryable();

            if (hasTitle) q = q.Where(b => b.Title.Contains(title));
            if (hasAuthor) q = q.Where(b => b.Author.Contains(author));
            if (hasYear) q = q.Where(b => b.Year == year);
            if (hasTag) q = q.Where(b => (b.Tags & tagEnum) != BookTags.None);
            if (hasCats) q = q.Where(b => b.CategoryId.HasValue && selectedCategoryIds.Contains(b.CategoryId.Value));

            var books = await q
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Author)
                .ToListAsync();

            ViewBag.TitleFilter = title;
            ViewBag.Author = author;
            ViewBag.Year = year;
            ViewBag.Tag = tag;
            ViewBag.CategoryIds = string.Join(",", selectedCategoryIds);

            ViewBag.AllCategories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.ParentCategoryId == null ? 0 : 1)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewBag.TitleSuggestions = await _context.Books
                .AsNoTracking()
                .Where(b => b.IsActive)
                .Select(b => b.Title)
                .Distinct()
                .OrderBy(x => x)
                .Take(300)
                .ToListAsync();

            ViewBag.AuthorSuggestions = await _context.Books
                .AsNoTracking()
                .Where(b => b.IsActive)
                .Select(b => b.Author)
                .Distinct()
                .OrderBy(x => x)
                .Take(300)
                .ToListAsync();

            return View(books);
        }

        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(b => b.Category)
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound();

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

                return View(vm);
            }

            using var tx = await _context.Database.BeginTransactionAsync();

            var book = new Book
            {
                Title = vm.Title,
                Author = vm.Author,
                Year = vm.Year,
                Description = vm.Description,
                CoverUrl = vm.CoverUrl,
                CategoryId = finalCategoryId!.Value,

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
    }
}