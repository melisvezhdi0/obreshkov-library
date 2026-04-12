#nullable enable

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
    public class readersController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TemporaryPasswordService _temporaryPasswordService;

        public readersController(
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
            var readers = await _context.Readers
                .Where(c => c.IsActive)
                .ToListAsync();
            var availableClasses = readers
     .Where(c => c.Grade.HasValue)
     .Select(c => new
     {
         Grade = c.Grade.Value,
         Section = (c.Section ?? "").Trim().ToUpper()
     })
     .Distinct()
     .OrderBy(c => c.Grade)
     .ThenBy(c => c.Section)
     .Select(c => string.IsNullOrWhiteSpace(c.Section)
         ? c.Grade.ToString()
         : $"{c.Grade}{c.Section}")
     .ToList();

            if (!string.IsNullOrWhiteSpace(classFilter))
            {
                var normalizedClass = classFilter.Trim().Replace(" ", "").ToUpper();

                readers = readers
                    .Where(c =>
                    {
                        var grade = c.Grade.HasValue ? c.Grade.Value.ToString() : "";
                        var section = (c.Section ?? "").Trim();

                        var readerClass = string.IsNullOrWhiteSpace(section)
                            ? grade
                            : $"{grade}{section}".Replace(" ", "").ToUpper();

                        return readerClass == normalizedClass;
                    })
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();

                readers = readers
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

            readers = readers
                .OrderBy(c => c.Grade ?? int.MaxValue)
                .ThenBy(c => c.Section ?? "")
                .ThenBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToList();

            ViewBag.Search = search ?? "";
            ViewBag.ClassFilter = classFilter ?? "";
            ViewBag.AvailableClasses = availableClasses;

            return View(readers);
        }

        public async Task<IActionResult> Archived(string? search, string? classFilter)
        {
            var q = _context.Readers
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

            var readers = await q
                .OrderBy(c => c.Grade ?? int.MaxValue)
                .ThenBy(c => c.Section ?? "")
                .ThenBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.ClassFilter = classFilter;

            return View(readers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reader = await _context.Readers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reader == null) return NotFound();

            var activeLoans = await _context.Loans
                .Where(l => l.ReaderId == reader.Id && l.ReturnDate == null)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();

            ViewBag.ActiveLoans = activeLoans;

            string passwordDisplay;

            if (string.IsNullOrWhiteSpace(reader.LastTemporaryPassword))
            {
                passwordDisplay = "Няма данни";
            }
            else if (reader.PasswordChangedByReader)
            {
                passwordDisplay = "Успешно сменена от ученика";
            }
            else
            {
                passwordDisplay = reader.LastTemporaryPassword;
            }

            ViewBag.PasswordDisplay = passwordDisplay;

            return View(reader);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,MiddleName,LastName,PhoneNumber,Grade,Section")] Reader reader)
        {
            reader.CardNumber = await GenerateUniqueCardNumberAsync();
            reader.CreatedOn = DateTime.Now;
            reader.IsActive = true;

            ModelState.Clear();
            TryValidateModel(reader);

            if (!ModelState.IsValid)
                return View(reader);

            _context.Add(reader);
            await _context.SaveChangesAsync();

            var generatedPassword = _temporaryPasswordService.Generate();
            reader.LastTemporaryPassword = generatedPassword;
            reader.PasswordChangedByReader = false;
            reader.LastPasswordChangeOn = null;

            _context.Update(reader);
            await _context.SaveChangesAsync();

            var ReaderUser = new IdentityUser
            {
                UserName = reader.CardNumber.Trim().ToUpper(),
                Email = $"Reader_{reader.CardNumber.Trim().Replace("-", "").ToLower()}@obreshkov.local",
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(ReaderUser, generatedPassword);

            if (!createUserResult.Succeeded)
            {
                _context.Readers.Remove(reader);
                await _context.SaveChangesAsync();

                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(reader);
            }

            var addRoleResult = await _userManager.AddToRoleAsync(ReaderUser, "Reader");

            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(ReaderUser);
                _context.Readers.Remove(reader);
                await _context.SaveChangesAsync();

                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(reader);
            }

            TempData["CreatedReaderPassword"] = generatedPassword;
            TempData["CreatedReaderCard"] = reader.CardNumber;
            TempData["CreatedReaderName"] = $"{reader.FirstName} {reader.LastName}";

            return RedirectToAction(nameof(Details), new { id = reader.Id });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var reader = await _context.Readers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (reader == null)
                return NotFound();

            return View(reader);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var readerToUpdate = await _context.Readers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (readerToUpdate == null)
                return NotFound();

            if (await TryUpdateModelAsync(readerToUpdate, "",
                c => c.FirstName,
                c => c.MiddleName,
                c => c.LastName,
                c => c.PhoneNumber,
                c => c.Grade,
                c => c.Section))
            {
                if (readerToUpdate.Grade.HasValue)
                {
                    if (readerToUpdate.Grade.Value >= 5 && readerToUpdate.Grade.Value <= 7)
                    {
                        readerToUpdate.Section = null;
                    }

                    if (readerToUpdate.Grade.Value >= 8 &&
                        readerToUpdate.Grade.Value <= 12 &&
                        string.IsNullOrWhiteSpace(readerToUpdate.Section))
                    {
                        ModelState.AddModelError("Section", "За 8. до 12. клас паралелката е задължителна.");
                        return View(readerToUpdate);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!readerExists(readerToUpdate.Id))
                        return NotFound();

                    throw;
                }
            }

            return View(readerToUpdate);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var reader = await _context.Readers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reader == null)
                return NotFound();

            return View(reader);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reader = await _context.Readers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (reader != null)
                reader.IsActive = false;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int Id)
        {
            var reader = await _context.Readers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (reader == null) return NotFound();

            reader.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int Id)
        {
            var reader = await _context.Readers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (reader == null) return NotFound();

            reader.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Archived));
        }

        public async Task<IActionResult> PasswordResetRequests()
        {
            var requests = await _context.PasswordResetRequests
                .Include(r => r.Reader)
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
                .Include(r => r.Reader)
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

            if (string.IsNullOrWhiteSpace(request.Reader.CardNumber))
            {
                TempData["Error"] = "Клиентът няма читателска карта.";
                return RedirectToAction(nameof(PasswordResetRequests));
            }

            var normalizedCardNumber = request.Reader.CardNumber.Trim().ToUpper();

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

            request.Reader.LastTemporaryPassword = tempPassword;
            request.Reader.PasswordChangedByReader = false;
            request.Reader.LastPasswordChangeOn = null;

            request.GeneratedPassword = tempPassword;
            request.IsCompleted = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Нова временна парола: {tempPassword}";
            return RedirectToAction(nameof(PasswordResetRequests));
        }
        private bool readerExists(int id)
        {
            return _context.Readers
                .IgnoreQueryFilters()
                .Any(e => e.Id == id);
        }

        private async Task<string> GenerateUniqueCardNumberAsync()
        {
            for (int attempt = 0; attempt < 30; attempt++)
            {
                var number = Random.Shared.Next(100000, 1000000).ToString();

                bool exists = await _context.Readers
                    .IgnoreQueryFilters()
                    .AnyAsync(c => c.CardNumber == number);

                if (!exists)
                    return number;
            }

            throw new InvalidOperationException("Неуспешно генериране на уникален номер на карта.");
        }

        public async Task PromoteReadersAsync()
        {
            var Readers = await _context.Readers
                .Where(c => c.IsActive && c.Grade != null)
                .ToListAsync();

            foreach (var Reader in Readers)
            {
                if (Reader.Grade < 12)
                    Reader.Grade++;
                else
                    Reader.IsActive = false;
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