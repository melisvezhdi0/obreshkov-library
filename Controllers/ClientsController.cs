using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Services;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientsController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TemporaryPasswordService _temporaryPasswordService;

        public ClientsController(
            ObreshkovLibraryContext context,
            UserManager<IdentityUser> userManager,
            TemporaryPasswordService temporaryPasswordService)
        {
            _context = context;
            _userManager = userManager;
            _temporaryPasswordService = temporaryPasswordService;
        }

        public async Task<IActionResult> Index(string? search, string? classFilter)
        {
            var clients = await _context.Clients
                .Where(c => c.IsActive)
                .ToListAsync();

            var availableClasses = clients
                .Where(c => c.Grade.HasValue)
                .Select(c =>
                {
                    var grade = c.Grade.Value.ToString();
                    var section = (c.Section ?? "").Trim();

                    return string.IsNullOrWhiteSpace(section)
                        ? grade
                        : $"{grade}{section}".Replace(" ", "").ToUpper();
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (!string.IsNullOrWhiteSpace(classFilter))
            {
                var normalizedClass = classFilter.Trim().Replace(" ", "").ToUpper();

                clients = clients
                    .Where(c =>
                    {
                        var grade = c.Grade.HasValue ? c.Grade.Value.ToString() : "";
                        var section = (c.Section ?? "").Trim();

                        var clientClass = string.IsNullOrWhiteSpace(section)
                            ? grade
                            : $"{grade}{section}".Replace(" ", "").ToUpper();

                        return clientClass == normalizedClass;
                    })
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();

                clients = clients
                    .Where(c =>
                    {
                        var fullName = $"{c.FirstName} {c.MiddleName} {c.LastName}"
                            .Replace("  ", " ")
                            .Trim();

                        return
                            fullName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                            (c.PhoneNumber ?? "").Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                            (c.CardNumber ?? "").Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();
            }

            clients = clients
                .OrderBy(c => c.Grade ?? int.MaxValue)
                .ThenBy(c => c.Section ?? "")
                .ThenBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList();

            ViewBag.Search = search ?? "";
            ViewBag.ClassFilter = classFilter ?? "";
            ViewBag.AvailableClasses = availableClasses;

            return View(clients);
        }

        public async Task<IActionResult> Archived(string? search, string? classFilter)
        {
            var q = _context.Clients
                .IgnoreQueryFilters()
                .Where(c => !c.IsActive)
                .AsQueryable();

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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null) return NotFound();

            var activeLoans = await _context.Loans
                .Where(l => l.ClientId == client.Id && l.ReturnDate == null)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            ViewBag.ActiveLoans = activeLoans;

            string passwordDisplay;

            if (string.IsNullOrWhiteSpace(client.LastTemporaryPassword))
            {
                passwordDisplay = "Няма данни";
            }
            else if (client.PasswordChangedByStudent)
            {
                passwordDisplay = "Успешно сменена от ученика";
            }
            else
            {
                passwordDisplay = client.LastTemporaryPassword;
            }

            ViewBag.PasswordDisplay = passwordDisplay;

            return View(client);
        }

        public IActionResult Create()
        {
            return View();
        }

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

            var generatedPassword = _temporaryPasswordService.Generate();
            client.LastTemporaryPassword = generatedPassword;
            client.PasswordChangedByStudent = false;
            client.LastPasswordChangeOn = null;

            _context.Update(client);
            await _context.SaveChangesAsync();

            var studentUser = new IdentityUser
            {
                UserName = client.CardNumber.Trim().ToUpper(),
                Email = $"student_{client.CardNumber.Trim().Replace("-", "").ToLower()}@obreshkov.local",
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(studentUser, generatedPassword);

            if (!createUserResult.Succeeded)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(client);
            }

            var addRoleResult = await _userManager.AddToRoleAsync(studentUser, "Student");

            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(studentUser);
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(client);
            }

            TempData["CreatedStudentPassword"] = generatedPassword;
            TempData["CreatedStudentCard"] = client.CardNumber;
            TempData["CreatedStudentName"] = $"{client.FirstName} {client.LastName}";

            return RedirectToAction(nameof(Details), new { id = client.Id });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var clientToUpdate = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clientToUpdate == null)
                return NotFound();

            if (await TryUpdateModelAsync(clientToUpdate, "",
                c => c.FirstName,
                c => c.MiddleName,
                c => c.LastName,
                c => c.PhoneNumber,
                c => c.Grade,
                c => c.Section))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(clientToUpdate.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(clientToUpdate);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var client = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
                return NotFound();

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client != null)
                client.IsActive = false;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int Id)
        {
            var client = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (client == null) return NotFound();

            client.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int Id)
        {
            var client = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (client == null) return NotFound();

            client.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Archived));
        }

        public async Task<IActionResult> PasswordResetRequests()
        {
            var requests = await _context.PasswordResetRequests
                .Include(r => r.Client)
                .OrderBy(r => r.IsCompleted)
                .ThenByDescending(r => r.RequestedOn)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateTemporaryPassword(int id)
        {
            var request = await _context.PasswordResetRequests
                .Include(r => r.Client)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["Error"] = "Заявката не е намерена.";
                return RedirectToAction(nameof(PasswordResetRequests));
            }

            if (request.IsCompleted)
            {
                TempData["Error"] = "Тази заявка вече е обработена.";
                return RedirectToAction(nameof(PasswordResetRequests));
            }

            if (string.IsNullOrWhiteSpace(request.Client.CardNumber))
            {
                TempData["Error"] = "Клиентът няма читателска карта.";
                return RedirectToAction(nameof(PasswordResetRequests));
            }

            var normalizedCardNumber = request.Client.CardNumber.Trim().ToUpper();

            var user = await _userManager.FindByNameAsync(normalizedCardNumber);
            if (user == null)
            {
                TempData["Error"] = "Не е намерен потребител за този ученик.";
                return RedirectToAction(nameof(PasswordResetRequests));
            }

            var tempPassword = _temporaryPasswordService.Generate();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, tempPassword);

            if (!resetResult.Succeeded)
            {
                TempData["Error"] = string.Join(" ", resetResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(PasswordResetRequests));
            }

            request.Client.LastTemporaryPassword = tempPassword;
            request.Client.PasswordChangedByStudent = false;
            request.Client.LastPasswordChangeOn = null;

            request.GeneratedPassword = tempPassword;
            request.IsCompleted = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Нова временна парола: {tempPassword}";
            return RedirectToAction(nameof(PasswordResetRequests));
        }
        private bool ClientExists(int id)
        {
            return _context.Clients
                .IgnoreQueryFilters()
                .Any(e => e.Id == id);
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
        private string GenerateTemporaryPasswordValue()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
    }
}