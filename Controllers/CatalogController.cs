using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObreshkovLibrary.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public CatalogController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId, string? sort)
        {
            sort ??= "date_desc";

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var categoryLookup = categories.ToLookup(c => c.ParentCategoryId);
            var selectedCategoryIds = new HashSet<int>();

            void AddCategoryAndChildren(int currentId)
            {
                if (!selectedCategoryIds.Add(currentId))
                    return;

                foreach (var child in categoryLookup[currentId])
                {
                    AddCategoryAndChildren(child.Id);
                }
            }

            if (categoryId.HasValue && categories.Any(c => c.Id == categoryId.Value))
            {
                AddCategoryAndChildren(categoryId.Value);
            }

            var booksQuery = _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (selectedCategoryIds.Count > 0)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId.HasValue && selectedCategoryIds.Contains(b.CategoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim().ToLower();

                booksQuery = booksQuery.Where(b =>
                    (b.Title != null && b.Title.ToLower().Contains(normalizedSearch)) ||
                    (b.Author != null && b.Author.ToLower().Contains(normalizedSearch)) ||
                    (b.Description != null && b.Description.ToLower().Contains(normalizedSearch)) ||
                    (b.Category != null && b.Category.Name.ToLower().Contains(normalizedSearch)) ||
                    (b.Category != null &&
                     b.Category.ParentCategory != null &&
                     b.Category.ParentCategory.Name.ToLower().Contains(normalizedSearch))
                );
            }

            booksQuery = sort switch
            {
                "date_asc" => booksQuery.OrderBy(b => b.CreatedOn).ThenBy(b => b.Title),
                "name_asc" => booksQuery.OrderBy(b => b.Title).ThenBy(b => b.Author),
                "name_desc" => booksQuery.OrderByDescending(b => b.Title).ThenBy(b => b.Author),
                _ => booksQuery.OrderByDescending(b => b.CreatedOn).ThenBy(b => b.Title)
            };

            var selectedCategoryName = "Всички книги";
            if (categoryId.HasValue)
            {
                var selectedCategory = categories.FirstOrDefault(c => c.Id == categoryId.Value);
                if (selectedCategory != null)
                {
                    selectedCategoryName = selectedCategory.Name;
                }
            }

            var vm = new CatalogIndexVM
            {
                Books = await booksQuery.ToListAsync(),
                Categories = categories,
                Search = search?.Trim() ?? string.Empty,
                Sort = sort,
                SelectedCategoryId = categoryId,
                SelectedCategoryName = selectedCategoryName
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .AsNoTracking()
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);

            if (book == null)
            {
                return NotFound();
            }

            var activeCopyIds = book.Copies?
                .Where(c => c.IsActive)
                .Select(c => c.Id)
                .ToList() ?? new List<int>();

            var activeLoanCopyIds = await _context.Loans
                .AsNoTracking()
                .Where(l => activeCopyIds.Contains(l.BookCopyId) && l.ReturnDate == null)
                .Select(l => l.BookCopyId)
                .ToListAsync();

            var isAvailable = book.Copies != null &&
                              book.Copies.Any(c => c.IsActive && !activeLoanCopyIds.Contains(c.Id));

            var authorBooks = await _context.Books
                .AsNoTracking()
                .Where(b => b.IsActive &&
                            b.Id != book.Id &&
                            b.Author == book.Author)
                .OrderByDescending(b => b.CreatedOn)
                .ThenBy(b => b.Title)
                .Take(10)
                .ToListAsync();

            var allCandidates = await _context.Books
                .AsNoTracking()
                .Where(b => b.IsActive && b.Id != book.Id)
                .ToListAsync();

            var currentTagValues = GetTagValues(book.Tags);
            var minimumMatches = currentTagValues.Count >= 2 ? 2 : currentTagValues.Count;

            var similarBooks = new List<Book>();

            if (minimumMatches > 0)
            {
                similarBooks = allCandidates
                    .Where(b => CountMatchingTags(book.Tags, b.Tags) >= minimumMatches)
                    .OrderBy(_ => Guid.NewGuid())
                    .Take(10)
                    .ToList();
            }

            ViewBag.IsAvailable = isAvailable;
            ViewBag.RelatedBooks = authorBooks;
            ViewBag.SimilarBooks = similarBooks;

            return View(book);
        }

        private static List<BookTags> GetTagValues(BookTags tags)
        {
            return Enum.GetValues(typeof(BookTags))
                .Cast<BookTags>()
                .Where(t => t != BookTags.None && tags.HasFlag(t))
                .ToList();
        }

        private static int CountMatchingTags(BookTags first, BookTags second)
        {
            var common = first & second;
            return Enum.GetValues(typeof(BookTags))
                .Cast<BookTags>()
                .Count(t => t != BookTags.None && common.HasFlag(t));
        }
    }
}