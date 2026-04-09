using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Data.Seed;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Services;

namespace ObreshkovLibrary
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentException("Connection string was not found.");

            builder.Services.AddDbContext<ObreshkovLibraryContext>(options =>
                options.UseSqlServer(connectionString));

            var keysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
            Directory.CreateDirectory(keysPath);

            builder.Services
                .AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
                .SetApplicationName("ObreshkovLibrary");
 
            builder.Services
                .AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequiredLength = 6;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ObreshkovLibraryContext>();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Home/Index";

                options.Cookie.Name = "ObreshkovLibrary.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Path = "/";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.Cookie.MaxAge = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
            });

            builder.Services.AddScoped<CardNumberGenerator>();
            builder.Services.AddScoped<BookDeactivateService>();
            builder.Services.AddScoped<TemporaryPasswordService>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.Name = "ObreshkovLibrary.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Path = "/";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            builder.Services.AddScoped<IStudentNotificationService, StudentNotificationService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;
                var method = context.Request.Method;

                if (path.StartsWithSegments("/gate") ||
                    path.StartsWithSegments("/css") ||
                    path.StartsWithSegments("/js") ||
                    path.StartsWithSegments("/lib") ||
                    path.StartsWithSegments("/images") ||
                    path.StartsWithSegments("/favicon"))
                {
                    await next();
                    return;
                }

                if (path.StartsWithSegments("/Categories/QuickCreate") ||
                    path.StartsWithSegments("/Categories/Create") ||
                    path.StartsWithSegments("/Categories/CreatePath"))
                {
                    await next();
                    return;
                }

                bool needsGate =
                    HttpMethods.IsPost(method) &&
                    (
                        path.StartsWithSegments("/Clients/Deactivate") ||
                        path.StartsWithSegments("/Categories/Deactivate") ||
                        path.StartsWithSegments("/Book/Deactivate") 
                    );

                if (needsGate)
                {
                    if (!context.User.Identity!.IsAuthenticated &&
                        context.Session.GetString("GateOk") != "1")
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

            app.MapRazorPages();

            await SeedData.InitializeAsync(app.Services);

            app.Run();
        }
    }
}