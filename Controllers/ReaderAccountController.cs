using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Reader")]
    public class ReaderAccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ObreshkovLibraryContext _context;

        public ReaderAccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ObreshkovLibraryContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ReaderChangePasswordVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["PasswordChangeError"] = "Моля, попълни правилно всички полета.";
                return RedirectToAction("Index", "ReaderPortal");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                TempData["PasswordChangeError"] = "Паролата трябва да е поне 6 символа и да съдържа една главна буква и една цифра.";
                return RedirectToAction("Index", "ReaderPortal");
            }

            var cardNumber = user.UserName?.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                var reader = await _context.Readers
                    .FirstOrDefaultAsync(c => c.CardNumber != null && c.CardNumber.ToUpper() == cardNumber);

                if (reader != null)
                {
                    reader.PasswordChangedByReader = true;
                    reader.LastPasswordChangeOn = DateTime.Now;
                    reader.LastTemporaryPassword = null;
                    await _context.SaveChangesAsync();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Паролата е сменена успешно.";

            return RedirectToAction("Index", "ReaderPortal");
        }
    }
}