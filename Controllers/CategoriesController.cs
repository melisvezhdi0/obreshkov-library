using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly ICategoryService _categoryService;

        public CategoriesController(
            ObreshkovLibraryContext context,
            ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _categoryService.GetIndexDataAsync();
            ViewBag.DirectBookCounts = data.DirectBookCounts;
            return View(data.Categories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryService.GetDetailsAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        public IActionResult Create()
        {
            return RedirectToAction(nameof(CreatePath));
        }

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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Name", category.ParentCategoryId);
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _categoryService.DeactivateAsync(id);

            if (!result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                {
                    TempData["Error"] = result.ErrorMessage;
                    return RedirectToAction(nameof(Edit), new { id });
                }

                return NotFound();
            }

            TempData["Success"] = "Категорията е преместена в архива.";
            return RedirectToAction(nameof(Edit), new { id });
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

            await _categoryService.CreatePathAsync(vm);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreate(string name, int? parentCategoryId)
        {
            name = (name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Името е задължително.");

            var result = await _categoryService.QuickCreateAsync(name, parentCategoryId);

            return Json(new { id = result.Id, name = result.Name });
        }

        [HttpGet]
        public async Task<IActionResult> GetRoots()
        {
            var items = await _categoryService.GetRootsAsync();
            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetChildren(int parentId)
        {
            var items = await _categoryService.GetChildrenAsync(parentId);
            return Json(items);
        }
    }
}