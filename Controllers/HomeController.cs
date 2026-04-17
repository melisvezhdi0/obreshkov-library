using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;

namespace ObreshkovLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public HomeController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var latestNews = await _context.SchoolNews
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.PublishedOn)
                .ThenByDescending(x => x.CreatedOn)
                .Take(3)
                .ToListAsync();

            var latestBooks = await _context.Books
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedOn)
                .ThenBy(b => b.Title)
                .Take(5)
                .ToListAsync();

            ViewBag.HomeNews = latestNews;
            ViewBag.LatestBooks = latestBooks;

            return View();
        }

        [Authorize(Roles = "Reader")]
        [HttpGet]
        public async Task<IActionResult> ReaderIndex()
        {
            var latestNews = await _context.SchoolNews
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.PublishedOn)
                .ThenByDescending(x => x.CreatedOn)
                .Take(3)
                .ToListAsync();

            var latestBooks = await _context.Books
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedOn)
                .ThenBy(b => b.Title)
                .Take(5)
                .ToListAsync();

            ViewBag.HomeNews = latestNews;
            ViewBag.LatestBooks = latestBooks;

            return View();
        }
    }
}