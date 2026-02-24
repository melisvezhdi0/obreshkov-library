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

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromHours(2);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
            });

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
            app.UseSession();

            app.Use(async (context, next) =>
            {
                var path = (context.Request.Path.Value ?? "").ToLowerInvariant();
                var method = (context.Request.Method ?? "GET").ToUpperInvariant();

                if (path.StartsWith("/gate") ||
                    path.StartsWith("/css") || path.StartsWith("/js") ||
                    path.StartsWith("/lib") || path.StartsWith("/images") ||
                    path.StartsWith("/favicon"))
                {
                    await next();
                    return;
                }

                bool isWrite = method != "GET";

                if (isWrite)
                {
                    if (context.Session.GetString("GateOk") != "1")
                    {
                        context.Response.StatusCode = 403;
                        return;
                    }
                }

                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}