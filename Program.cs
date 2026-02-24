using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Services;

namespace ObreshkovLibrary
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // SQLite file in project folder (App_Data)
            var dbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "ObreshkovLibrary.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            builder.Services.AddDbContext<ObreshkovLibraryContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddScoped<CardNumberGenerator>();
            builder.Services.AddScoped<BookDeactivateService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}