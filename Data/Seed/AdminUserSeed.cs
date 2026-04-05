using Microsoft.AspNetCore.Identity;

namespace ObreshkovLibrary.Data.Seed
{
    public static class AdminUserSeed
    {
        public static async Task SeedAdminAsync(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "admin@obreshkov.bg";
            const string adminPassword = "Admin123!";
            const string adminRole = "Admin";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var existingUser = await userManager.FindByEmailAsync(adminEmail);
            if (existingUser != null)
            {
                if (!await userManager.IsInRoleAsync(existingUser, adminRole))
                {
                    await userManager.AddToRoleAsync(existingUser, adminRole);
                }

                return;
            }

            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                throw new Exception($"Admin user seed failed: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}