using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data.Seed
{
    public static class CategorySeed
    {
        public static async Task SeedCategoriesAsync(ObreshkovLibraryContext context)
        {
            var fiction = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Name == "Художествена литература" && c.ParentCategoryId == null);

            if (fiction == null)
            {
                fiction = new Category
                {
                    Name = "Художествена литература",
                    IsActive = true
                };

                context.Categories.Add(fiction);
                await context.SaveChangesAsync();
            }

            var children = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Name == "Детска литература" && c.ParentCategoryId == null);

            if (children == null)
            {
                children = new Category
                {
                    Name = "Детска литература",
                    IsActive = true
                };

                context.Categories.Add(children);
                await context.SaveChangesAsync();
            }

            var science = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Name == "Научнопопулярна литература" && c.ParentCategoryId == null);

            if (science == null)
            {
                science = new Category
                {
                    Name = "Научнопопулярна литература",
                    IsActive = true
                };

                context.Categories.Add(science);
                await context.SaveChangesAsync();
            }

            var school = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Name == "Учебна литература" && c.ParentCategoryId == null);

            if (school == null)
            {
                school = new Category
                {
                    Name = "Учебна литература",
                    IsActive = true
                };

                context.Categories.Add(school);
                await context.SaveChangesAsync();
            }

            async Task AddChildIfMissing(string name, int parentId)
            {
                bool exists = await context.Categories
                    .IgnoreQueryFilters()
                    .AnyAsync(c => c.Name == name && c.ParentCategoryId == parentId);

                if (!exists)
                {
                    context.Categories.Add(new Category
                    {
                        Name = name,
                        ParentCategoryId = parentId,
                        IsActive = true
                    });
                }
            }

            await AddChildIfMissing("Роман", fiction.Id);
            await AddChildIfMissing("Разказ", fiction.Id);
            await AddChildIfMissing("Стихотворение", fiction.Id);

            await AddChildIfMissing("Приказка", children.Id);
            await AddChildIfMissing("Психология", science.Id);
            await AddChildIfMissing("Учебник", school.Id);
            await AddChildIfMissing("Помагало", school.Id);

            await context.SaveChangesAsync();
        }
    }
}