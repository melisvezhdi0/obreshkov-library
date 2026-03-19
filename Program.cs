using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Services;
using Microsoft.AspNetCore.Identity;

namespace ObreshkovLibrary
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentException("Conection string was not found."); ;
            builder.Services.AddDbContext<ObreshkovLibraryContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ObreshkovLibraryContext>();

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
                        path.StartsWithSegments("/Categories/Deactivate")
                    );

                if (needsGate)
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

            app.MapRazorPages();

            app.Run();
        }
    }
}