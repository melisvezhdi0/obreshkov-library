using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ObreshkovLibraryContext>();

            await context.Database.MigrateAsync();

            var fictionCategory = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c =>
                    c.Name == "Художествена литература" &&
                    c.ParentCategoryId == null);

            if (fictionCategory == null)
            {
                fictionCategory = new Category
                {
                    Name = "Художествена литература",
                    IsActive = true
                };

                context.Categories.Add(fictionCategory);
                await context.SaveChangesAsync();
            }

            var novelsCategory = await context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c =>
                    c.Name == "Романи" &&
                    c.ParentCategoryId == fictionCategory.Id);

            if (novelsCategory == null)
            {
                novelsCategory = new Category
                {
                    Name = "Романи",
                    ParentCategoryId = fictionCategory.Id,
                    IsActive = true
                };

                context.Categories.Add(novelsCategory);
                await context.SaveChangesAsync();
            }

            var existingBook = await context.Books
                .IgnoreQueryFilters()
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(b =>
                    b.Title == "Железният светилник" &&
                    b.Author == "Димитър Талев");

            if (existingBook == null)
            {
                var book = new Book
                {
                    Title = "Железният светилник",
                    Author = "Димитър Талев",
                    Year = 1952,
                    Description = "„Железният светилник“ е исторически роман и първата книга от известната тетралогия на Димитър Талев („Железният светилник“, „Преспанските камбани“, „Илинден“ и „Гласовете ви чувам“).",
                    CoverPath = "/uploadsimages/zhelezniat-svetilnik.jpg",
                    CategoryId = novelsCategory.Id,
                    Tags = BookTags.Bulgarian | BookTags.SchoolLiterature,
                    IsActive = true
                };

                context.Books.Add(book);
                await context.SaveChangesAsync();

                var copies = new List<BookCopy>
                {
                    new BookCopy { BookId = book.Id, IsActive = true },
                    new BookCopy { BookId = book.Id, IsActive = true },
                    new BookCopy { BookId = book.Id, IsActive = true }
                };

                context.BookCopies.AddRange(copies);
                await context.SaveChangesAsync();
            }
            else
            {
                existingBook.Year = 1952;
                existingBook.Description = "„Железният светилник“ е исторически роман и първата книга от известната тетралогия на Димитър Талев („Железният светилник“, „Преспанските камбани“, „Илинден“ и „Гласовете ви чувам“).";
                existingBook.CoverPath = "/uploadsimages/zhelezniat-svetilnik.jpg";
                existingBook.CategoryId = novelsCategory.Id;
                existingBook.Tags = BookTags.Bulgarian | BookTags.SchoolLiterature;
                existingBook.IsActive = true;

                var copiesCount = await context.BookCopies
                    .IgnoreQueryFilters()
                    .CountAsync(c => c.BookId == existingBook.Id);

                if (copiesCount < 3)
                {
                    var missingCopies = 3 - copiesCount;

                    for (int i = 0; i < missingCopies; i++)
                    {
                        context.BookCopies.Add(new BookCopy
                        {
                            BookId = existingBook.Id,
                            IsActive = true
                        });
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}