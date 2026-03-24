using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BookController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly BookDeactivateService _bookDeactivate;
        private readonly IWebHostEnvironment _environment;

        public BookController(
            ObreshkovLibraryContext context,
            BookDeactivateService bookDeactivate,
            IWebHostEnvironment environment)
        {
            _context = context;
            _bookDeactivate = bookDeactivate;
            _environment = environment;
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

        private async Task PopulateBookVmAsync(BookCreateVM vm)
        {
            vm.Level1Options = await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            vm.TagOptions = BuildTagOptions();
        }

        private bool IsAllowedImageExtension(string extension)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            return allowed.Contains(extension.ToLowerInvariant());
        }

        private async Task<string?> SaveCoverFileAsync(IFormFile? coverFile)
        {
            if (coverFile == null || coverFile.Length == 0)
                return null;

            var extension = Path.GetExtension(coverFile.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !IsAllowedImageExtension(extension))
                return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploadsimages");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await coverFile.CopyToAsync(stream);

            return $"/uploadsimages/{fileName}";
        }

        private void DeleteCoverFile(string? coverPath)
        {
            if (string.IsNullOrWhiteSpace(coverPath) || !coverPath.StartsWith("/uploadsimages/"))
                return;

            var relativePath = coverPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var absolutePath = Path.Combine(_environment.WebRootPath, relativePath);

            if (System.IO.File.Exists(absolutePath))
                System.IO.File.Delete(absolutePath);
        }

        [HttpGet]
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? schoolClass, string? sort)
        {
            var query = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();

                query = query.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(normalizedSearch)) ||
                    (b.Author != null && b.Author.ToLower().Contains(normalizedSearch)) ||
                    (b.Category != null && b.Category.Name.ToLower().Contains(normalizedSearch)) ||
                    (b.Category != null &&
                     b.Category.ParentCategory != null &&
                     b.Category.ParentCategory.Name.ToLower().Contains(normalizedSearch)) ||
                    (b.Description != null && b.Description.ToLower().Contains(normalizedSearch))
                );
            }

            query = sort switch
            {
                "name_desc" => query.OrderByDescending(b => b.Title).ThenBy(b => b.Author),
                "date_asc" => query.OrderBy(b => b.CreatedOn).ThenBy(b => b.Title),
                "date_desc" => query.OrderByDescending(b => b.CreatedOn).ThenBy(b => b.Title),
                _ => query.OrderBy(b => b.Title).ThenBy(b => b.Author)
            };

            var books = await query.ToListAsync();

            ViewBag.Search = search ?? "";
            ViewBag.SchoolClass = schoolClass ?? "";
            ViewBag.Sort = sort ?? "name_asc";

            return View(books);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Recent(string? search, string? sort)
        {
            var monthAgo = DateTime.Now.AddMonths(-1);

            var query = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(b => b.IsActive && b.CreatedOn >= monthAgo)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();

                query = query.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(normalizedSearch)) ||
                    (b.Author != null && b.Author.ToLower().Contains(normalizedSearch)) ||
                    (b.Category != null && b.Category.Name.ToLower().Contains(normalizedSearch)) ||
                    (b.Category != null &&
                     b.Category.ParentCategory != null &&
                     b.Category.ParentCategory.Name.ToLower().Contains(normalizedSearch)) ||
                    (b.Description != null && b.Description.ToLower().Contains(normalizedSearch))
                );
            }

            query = sort switch
            {
                "name_desc" => query.OrderByDescending(b => b.Title).ThenBy(b => b.Author),
                "date_asc" => query.OrderBy(b => b.CreatedOn).ThenBy(b => b.Title),
                _ => query.OrderByDescending(b => b.CreatedOn).ThenBy(b => b.Title)
            };

            var books = await query.ToListAsync();

            ViewBag.Search = search ?? "";
            ViewBag.Sort = sort ?? "date_desc";

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

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .IgnoreQueryFilters()
                .Include(b => b.Category)
                .ThenInclude(c => c.ParentCategory)
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
            var vm = new BookCreateVM();
            await PopulateBookVmAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateVM vm)
        {
            vm.Title = (vm.Title ?? string.Empty).Trim();
            vm.Author = (vm.Author ?? string.Empty).Trim();
            vm.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

            var finalCategoryId = vm.Level2Id ?? vm.Level1Id;

            if (!finalCategoryId.HasValue)
                ModelState.AddModelError("", "Моля, изберете категория.");

            if (vm.CoverFile != null)
            {
                var extension = Path.GetExtension(vm.CoverFile.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !IsAllowedImageExtension(extension))
                    ModelState.AddModelError(nameof(vm.CoverFile), "Позволени са само файлове: .jpg, .jpeg, .png, .webp");
            }

            if (!ModelState.IsValid)
            {
                await PopulateBookVmAsync(vm);
                return View(vm);
            }

            var savedCoverPath = await SaveCoverFileAsync(vm.CoverFile);

            using var tx = await _context.Database.BeginTransactionAsync();

            var book = new Book
            {
                Title = vm.Title,
                Author = vm.Author,
                Year = vm.Year,
                Description = vm.Description,
                CoverPath = savedCoverPath,
                CategoryId = finalCategoryId.Value,
                Tags = BuildTagsFromSelected(vm.SelectedTagValues),
                IsActive = true
            };

            book.CreatedOn = DateTime.Now;

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
                CoverPath = book.CoverPath,
                CurrentCoverPath = book.CoverPath,
                Level1Id = level1Id,
                Level2Id = level2Id,
                CopiesCount = Math.Max(1, await _context.BookCopies
                    .IgnoreQueryFilters()
                    .CountAsync(c => c.BookId == book.Id)),
                SelectedTagValues = TagsToSelectedValues(book.Tags)
            };

            await PopulateBookVmAsync(vm);

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

            vm.Title = (vm.Title ?? string.Empty).Trim();
            vm.Author = (vm.Author ?? string.Empty).Trim();
            vm.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

            var finalCategoryId = vm.Level2Id ?? vm.Level1Id;
            if (!finalCategoryId.HasValue)
                ModelState.AddModelError("", "Моля, изберете категория.");

            if (vm.CoverFile != null)
            {
                var extension = Path.GetExtension(vm.CoverFile.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !IsAllowedImageExtension(extension))
                    ModelState.AddModelError(nameof(vm.CoverFile), "Позволени са само файлове: .jpg, .jpeg, .png, .webp");
            }

            if (!ModelState.IsValid)
            {
                vm.CurrentCoverPath = book.CoverPath;
                await PopulateBookVmAsync(vm);
                ViewBag.BookId = book.Id;
                ViewBag.IsBookActive = book.IsActive;
                return View(vm);
            }

            var oldCoverPath = book.CoverPath;
            var newCoverPath = await SaveCoverFileAsync(vm.CoverFile);

            book.Title = vm.Title;
            book.Author = vm.Author;
            book.Year = vm.Year;
            book.Description = vm.Description;
            book.SchoolClass = string.IsNullOrWhiteSpace(vm.SchoolClass) ? null : vm.SchoolClass.Trim();
            book.CategoryId = finalCategoryId.Value;
            book.Tags = BuildTagsFromSelected(vm.SelectedTagValues);

            if (!string.IsNullOrWhiteSpace(newCoverPath))
            {
                book.CoverPath = newCoverPath;
                DeleteCoverFile(oldCoverPath);
            }

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