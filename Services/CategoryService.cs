using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ObreshkovLibraryContext _context;

        public CategoryService(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        public async Task<(List<Category> Categories, Dictionary<int, int> DirectBookCounts)> GetIndexDataAsync()
        {
            var categories = await _context.Categories
                .IgnoreQueryFilters()
                .OrderBy(c => c.Name)
                .ToListAsync();

            var directBookCounts = await _context.Books
                .Where(b => b.IsActive && b.CategoryId != null)
                .GroupBy(b => b.CategoryId!.Value)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            return (categories, directBookCounts);
        }

        public async Task<Category?> GetDetailsAsync(int id)
        {
            return await _context.Categories
                .IgnoreQueryFilters()
                .Include(c => c.ParentCategory)
                .Include(c => c.Children)
                .Include(c => c.Books)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string? ErrorMessage)> DeactivateAsync(int id)
        {
            var category = await _context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return (false, null);

            bool hasBooks = await _context.Books
                .IgnoreQueryFilters()
                .AnyAsync(b => b.CategoryId == id);

            if (hasBooks)
            {
                return (false, "Категорията не може да бъде деактивирана, защото в нея има книги.");
            }

            bool hasActiveChildren = await _context.Categories
                .IgnoreQueryFilters()
                .AnyAsync(c => c.ParentCategoryId == id && c.IsActive);

            if (hasActiveChildren)
            {
                return (false, "Категорията не може да бъде деактивирана, защото има активни подкатегории.");
            }

            category.IsActive = false;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task CreatePathAsync(CategoryPathVM vm)
        {
            var l1 = (vm.Level1 ?? "").Trim();
            var l2 = (vm.Level2 ?? "").Trim();

            var root = await _context.Categories
                .FirstOrDefaultAsync(c => c.ParentCategoryId == null && c.Name == l1);

            if (root == null)
            {
                root = new Category
                {
                    Name = l1,
                    ParentCategoryId = null,
                    IsActive = true
                };

                _context.Categories.Add(root);
                await _context.SaveChangesAsync();
            }

            var exists = await _context.Categories
                .FirstOrDefaultAsync(c => c.ParentCategoryId == root.Id && c.Name == l2);

            if (exists == null)
            {
                var child = new Category
                {
                    Name = l2,
                    ParentCategoryId = root.Id,
                    IsActive = true
                };

                _context.Categories.Add(child);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(int Id, string Name)> QuickCreateAsync(string name, int? parentCategoryId)
        {
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

                return (existing.Id, existing.Name);
            }

            var category = new Category
            {
                Name = name,
                ParentCategoryId = parentCategoryId,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return (category.Id, category.Name);
        }

        public async Task<List<object>> GetRootsAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .Cast<object>()
                .ToListAsync();
        }

        public async Task<List<object>> GetChildrenAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .Cast<object>()
                .ToListAsync();
        }
    }
}