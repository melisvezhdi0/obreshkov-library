using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ObreshkovLibrary.Controllers
{
    public class ArchiveController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public ArchiveController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            search = (search ?? "").Trim();
            ViewBag.Search = search;

            var booksQ = _context.Books
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(b => !b.IsActive)
                .AsQueryable();

            var clientsQ = _context.Clients
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsActive)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();

                var sNum = s.Replace(" ", "").Replace("-", "");

                booksQ = booksQ.Where(b =>
                    (b.Title ?? "").ToLower().Contains(s) ||
                    (b.Author ?? "").ToLower().Contains(s) ||
                    (b.Description ?? "").ToLower().Contains(s));

                clientsQ = clientsQ.Where(c =>
                    ((c.FirstName ?? "") + " " + (c.LastName ?? "")).ToLower().Contains(s) ||
                    (c.CardNumber ?? "").ToLower().Contains(s) ||
                    (c.CardNumber ?? "").Replace(" ", "").Replace("-", "").ToLower().Contains(sNum)
                );
            }

            var archivedBooks = await booksQ
                .OrderByDescending(b => b.Id)
                .Take(10)
                .ToListAsync();

            var archivedClients = await clientsQ
                .OrderByDescending(c => c.Id)
                .Take(10)
                .ToListAsync();

            ViewBag.ArchivedBooks = archivedBooks;
            ViewBag.ArchivedClients = archivedClients;
            ViewBag.HasResults = archivedBooks.Any() || archivedClients.Any();

            return View();
        }
    }
}