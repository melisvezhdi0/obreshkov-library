using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data.Seed
{
    public static class CategorySeed
    {
        public static async Task SeedCategoriesAsync(ObreshkovLibraryContext context)
        {
            if (await context.Categories.IgnoreQueryFilters().AnyAsync())
                return;

            var fiction = new Category
            {
                Name = "Художествена литература",
                IsActive = true
            };

            var children = new Category
            {
                Name = "Детска литература",
                IsActive = true
            };

            var science = new Category
            {
                Name = "Научнопопулярна литература",
                IsActive = true
            };

            var school = new Category
            {
                Name = "Учебна литература",
                IsActive = true
            };

            context.Categories.AddRange(fiction, children, science, school);
            await context.SaveChangesAsync();


            context.Categories.AddRange(
                new Category
                {
                    Name = "Роман",
                    ParentCategoryId = fiction.Id,
                    IsActive = true
                },
                new Category
                {
                    Name = "Разказ",
                    ParentCategoryId = fiction.Id,
                    IsActive = true
                },
                new Category
                {
                    Name = "Стихотворение",
                    ParentCategoryId = fiction.Id,
                    IsActive = true
                }
            );

            context.Categories.AddRange(
                new Category
                {
                    Name = "Приказка",
                    ParentCategoryId = children.Id,
                    IsActive = true
                },
                new Category
                {
                    Name = "Психология",
                    ParentCategoryId = science.Id,
                    IsActive = true
                },
                new Category
                {
                    Name = "Учебник",
                    ParentCategoryId = school.Id,
                    IsActive = true
                }
            );

            await context.SaveChangesAsync();
        }
    }
}