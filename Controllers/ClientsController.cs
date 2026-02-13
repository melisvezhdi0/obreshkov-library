using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public ClientsController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index(string? search, string? classFilter)
        {
            var q = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(classFilter))
            {
                var cf = classFilter.Trim().Replace(" ", "").ToUpper();

                q = q.Where(c =>
                    (((c.Grade != null ? c.Grade.ToString() : "") + (c.Section ?? ""))
                        .ToUpper()
                        .Replace(" ", ""))
                    == cf
                );
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                var sNoSpacesUpper = s.Replace(" ", "").ToUpper();

                q = q.Where(c =>
                    (c.FirstName ?? "").Contains(s) ||
                    (c.MiddleName ?? "").Contains(s) ||
                    (c.LastName ?? "").Contains(s) ||
                    (c.PhoneNumber ?? "").Contains(s) ||
                    (c.CardNumber ?? "").Contains(s) ||
                    (((c.Grade != null ? c.Grade.ToString() : "") + (c.Section ?? ""))
                        .ToUpper()
                        .Replace(" ", ""))
                        .Contains(sNoSpacesUpper)
                );
            }

            var clients = await q
                .OrderBy(c => c.Grade ?? int.MaxValue)
                .ThenBy(c => c.Section ?? "")
                .ThenBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.ClassFilter = classFilter;

            return View(clients);
        }


        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null) return NotFound();

            var activeLoans = await _context.Loans
                .Where(l => l.ClientId == client.Id && l.ReturnDate == null)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.BookTitle)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            ViewBag.ActiveLoans = activeLoans;

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,MiddleName,LastName,PhoneNumber,Grade,Section")] Client client)
        {
            client.CardNumber = await GenerateUniqueCardNumberAsync();
            client.CreatedOn = DateTime.Now;
            client.IsActive = true;

            ModelState.Clear();
            TryValidateModel(client);

            if (!ModelState.IsValid)
                return View(client);

            _context.Add(client);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = client.Id });
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,LastName,PhoneNumber,Grade,Section")] Client client)
        {
            if (id != client.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
                _context.Clients.Remove(client);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }

        private async Task<string> GenerateUniqueCardNumberAsync()
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                var number = Random.Shared.Next(100000, 1000000).ToString();

                bool exists = await _context.Clients
                    .IgnoreQueryFilters()
                    .AnyAsync(c => c.CardNumber == number);

                if (!exists)
                    return number;
            }

            throw new InvalidOperationException("Неуспешно генериране на уникален номер на карта.");
        }

        public async Task PromoteStudentsAsync()
        {
            var students = await _context.Clients
                .Where(c => c.IsActive && c.Grade != null)
                .ToListAsync();

            foreach (var student in students)
            {
                if (student.Grade < 12)
                    student.Grade++;
                else
                    student.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}
