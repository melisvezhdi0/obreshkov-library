#nullable enable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;

namespace ObreshkovLibrary.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ObreshkovLibraryContext _context;

        public ForgotPasswordModel(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Избери роля.")]
            public string Role { get; set; } = "Reader";

            public string? Email { get; set; }

            public string? CardNumber { get; set; }

            public string? PhoneNumber { get; set; }
        }

        public void OnGet(string? role = null)
        {
            Input.Role = string.IsNullOrWhiteSpace(role) ? "Reader" : role;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.Role == "Admin")
            {
                StatusMessage = "За администраторски профил се обърнете към системния администратор.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(Input.CardNumber))
            {
                ModelState.AddModelError(string.Empty, "Въведи номер на читателска карта.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.PhoneNumber))
            {
                ModelState.AddModelError(string.Empty, "Въведи телефонен номер.");
                return Page();
            }

            var normalizedCard = Input.CardNumber.Trim().ToUpper();
            var normalizedPhone = Input.PhoneNumber.Trim();

            var reader = await _context.Readers
                .FirstOrDefaultAsync(c =>
                    c.CardNumber.ToUpper() == normalizedCard &&
                    c.PhoneNumber == normalizedPhone);

            if (reader == null)
            {
                ModelState.AddModelError(string.Empty, "Няма съвпадение между читателската карта и телефонния номер.");
                return Page();
            }

            bool alreadyOpen = await _context.PasswordResetRequests
                .AnyAsync(r => r.ReaderId == reader.Id && !r.IsCompleted);

            if (!alreadyOpen)
            {
                var request = new PasswordResetRequest
                {
                    ReaderId = reader.Id,
                    CardNumber = reader.CardNumber,
                    PhoneNumber = reader.PhoneNumber,
                    RequestedOn = DateTime.Now,
                    IsCompleted = false
                };

                _context.PasswordResetRequests.Add(request);
                await _context.SaveChangesAsync();
            }

            StatusMessage = "Заявката е приета. Обърнете се към библиотекар или администратор за нова временна парола.";
            return RedirectToPage(new { role = "Reader" });
        }
    }
}