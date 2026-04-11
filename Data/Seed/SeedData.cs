using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ObreshkovLibrary.Data.Seed
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

            await RoleSeed.SeedRolesAsync(roleManager);
            await AdminUserSeed.SeedAdminAsync(userManager, roleManager);

            await CategorySeed.SeedCategoriesAsync(context);
            await readerSeed.SeedreadersAsync(context, userManager);
            await BookSeed.SeedBooksAsync(context);

            await LoanSeed.SeedLoansAsync(context);
            await LoanSeed.SeedArchivedLoansAsync(context);

            await ReaderNotificationSeed.SeedReaderNotificationsAsync(context);
        }
    }
}