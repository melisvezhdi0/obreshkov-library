using Microsoft.AspNetCore.Identity;
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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(context, userManager);
            await SeedLibraryDataAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager)
        {
            const string adminEmail = "admin@obreshkov.bg";
            const string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            const string studentCardNumber = "LIB-2026-0001";
            const string studentPassword = "Student123!";

            var client = await context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CardNumber == studentCardNumber);

            if (client == null)
            {
                client = new Client
                {
                    FirstName = "Мелис",
                    MiddleName = "Т.",
                    LastName = "Ученик",
                    PhoneNumber = "0888000000",
                    CardNumber = studentCardNumber,
                    Grade = 11,
                    Section = "А",
                    IsActive = true,
                    CreatedOn = DateTime.Now
                };

                context.Clients.Add(client);
                await context.SaveChangesAsync();
            }

            var studentUser = await userManager.FindByNameAsync(studentCardNumber);
            if (studentUser == null)
            {
                studentUser = new IdentityUser
                {
                    UserName = studentCardNumber,
                    Email = $"student_{studentCardNumber.Replace("-", "").ToLower()}@obreshkov.local",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studentUser, studentPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }
            else if (!await userManager.IsInRoleAsync(studentUser, "Student"))
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }

        private static async Task SeedLibraryDataAsync(ObreshkovLibraryContext context)
        {
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
                    Description = "„Железният светилник“ е исторически роман и първата книга от известната тетралогия на Димитър Талев.",
                    CoverPath = "/uploadsimages/zhelezniat-svetilnik.jpg",
                    CategoryId = novelsCategory.Id,
                    Tags = BookTags.Bulgarian | BookTags.SchoolLiterature,
                    IsActive = true
                };

                context.Books.Add(book);
                await context.SaveChangesAsync();

                context.BookCopies.AddRange(
                    new BookCopy { BookId = book.Id, IsActive = true },
                    new BookCopy { BookId = book.Id, IsActive = true },
                    new BookCopy { BookId = book.Id, IsActive = true }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}