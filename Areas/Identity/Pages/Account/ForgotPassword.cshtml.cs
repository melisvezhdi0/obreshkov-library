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

            [EmailAddress(ErrorMessage = "Въведи валиден имейл адрес.")]
            public string? Email { get; set; }

            public string? CardNumber { get; set; }

            public string? PhoneNumber { get; set; }
        }

        public void OnGet(string? role = null)
        {
            Input.Role = NormalizeRole(role);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Input.Role = NormalizeRole(Input.Role);

            if (Input.Role == "Admin")
            {
                if (string.IsNullOrWhiteSpace(Input.Email))
                {
                    ModelState.AddModelError(string.Empty, "Въведи служебен имейл.");
                    return Page();
                }

                if (!new EmailAddressAttribute().IsValid(Input.Email))
                {
                    ModelState.AddModelError(string.Empty, "Въведи валиден имейл адрес.");
                    return Page();
                }

                StatusMessage = "Заявката за администраторски профил е приета формално. Реално автоматично съобщение не се изпраща. Свържете се със системния администратор.";
                return RedirectToPage(new { role = "Admin" });
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

            var normalizedCard = Input.CardNumber.Trim().Replace(" ", "").ToUpper();
            var normalizedPhone = Input.PhoneNumber.Trim();

            var reader = await _context.Readers
                .FirstOrDefaultAsync(r =>
                    r.CardNumber != null &&
                    r.PhoneNumber != null &&
                    r.CardNumber.Replace(" ", "").ToUpper() == normalizedCard &&
                    r.PhoneNumber == normalizedPhone);

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
                    CardNumber = reader.CardNumber ?? string.Empty,
                    PhoneNumber = reader.PhoneNumber ?? string.Empty,
                    RequestedOn = DateTime.Now,
                    IsCompleted = false
                };

                _context.PasswordResetRequests.Add(request);
                await _context.SaveChangesAsync();
            }

            StatusMessage = alreadyOpen
                ? "Вече има активна заявка за този ученик. Обърнете се към библиотекар или администратор."
                : "Заявката е приета. Обърнете се към библиотекар или администратор за нова временна парола.";

            return RedirectToPage(new { role = "Reader" });
        }

        private static string NormalizeRole(string? role)
        {
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "Reader";
        }
    }
}