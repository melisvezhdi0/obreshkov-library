using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;


namespace ObreshkovLibrary.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public CategoriesController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return RedirectToAction(nameof(CreatePath));
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = new SelectList(
            _context.Categories.OrderBy(c => c.Name),
            "Id",
            "Name",
            category.ParentCategoryId);

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var category = await _context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            bool hasActiveBooks = await _context.BookTitleCategories
            .AnyAsync(x => x.CategoryId == id && x.BookTitle.IsActive);


            if (hasActiveBooks)
            {
                TempData["Error"] = "Категорията не може да бъде деактивирана, защото има книги в нея.";
                return RedirectToAction(nameof(Index));
            }

            bool hasActiveChildren = await _context.Categories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.ParentCategoryId == id && c.IsActive);

            if (hasActiveChildren)
            {
                TempData["Error"] = "Категорията не може да бъде деактивирана, защото има активни подкатегории.";
                return RedirectToAction(nameof(Index));
            }

            category.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        private bool CategoryExists(int id)
        {
            return _context.Categories
                .IgnoreQueryFilters()
                .Any(e => e.Id == id);
        }

        [HttpGet]
        public IActionResult CreatePath()
        {
            return View(new CategoryPathVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePath(CategoryPathVM vm)
        {
            var l1 = (vm.Level1 ?? "").Trim();
            var l2 = (vm.Level2 ?? "").Trim();

            if (string.IsNullOrWhiteSpace(l1))
            {
                ModelState.AddModelError(nameof(vm.Level1), "Избери категория.");
                return View(vm);
            }

            if (string.IsNullOrWhiteSpace(l2))
            {
                ModelState.AddModelError(nameof(vm.Level2), "Попълни подкатегория.");
                return View(vm);
            }

            var root = await _context.Categories
                .FirstOrDefaultAsync(c => c.ParentCategoryId == null && c.Name == l1);

            if (root == null)
            {
                root = new Category { Name = l1, ParentCategoryId = null, IsActive = true };
                _context.Categories.Add(root);
                await _context.SaveChangesAsync();
            }

            var exists = await _context.Categories
                .FirstOrDefaultAsync(c => c.ParentCategoryId == root.Id && c.Name == l2);

            if (exists == null)
            {
                var child = new Category { Name = l2, ParentCategoryId = root.Id, IsActive = true };
                _context.Categories.Add(child);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreate(string name, int? parentCategoryId)
        {
            name = (name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Името е задължително.");

            var existing = await _context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c =>
                    c.ParentCategoryId == parentCategoryId &&
                    c.Name.ToLower() == name.ToLower());

            if (existing != null)
            {
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    await _context.SaveChangesAsync();
                }

                return Json(new { id = existing.Id, name = existing.Name });
            }

            var category = new Category
            {
                Name = name,
                ParentCategoryId = parentCategoryId,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Json(new { id = category.Id, name = category.Name });
        }
        [HttpGet]
        public async Task<IActionResult> GetRoots()
        {
            var items = await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetChildren(int parentId)
        {
            var items = await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Json(items);
        }
    }
}
