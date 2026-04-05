using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentAccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ObreshkovLibraryContext _context;

        public StudentAccountController(
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
        public async Task<IActionResult> ChangePassword(StudentChangePasswordVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (!ModelState.IsValid)
            {
                TempData["PasswordChangeError"] = "Моля, попълни правилно всички полета.";
                return RedirectToAction("Index", "StudentPortal");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                TempData["PasswordChangeError"] = "Паролата трябва да е поне 6 символа и да съдържа една главна буква и една цифра.";
                return RedirectToAction("Index", "StudentPortal");
            }

            var cardNumber = user.UserName?.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(cardNumber))
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.CardNumber != null && c.CardNumber.ToUpper() == cardNumber);

                if (client != null)
                {
                    client.PasswordChangedByStudent = true;
                    client.LastPasswordChangeOn = DateTime.Now;
                    client.LastTemporaryPassword = null;
                    await _context.SaveChangesAsync();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Паролата е сменена успешно.";

            return RedirectToAction("Index", "StudentPortal");
        }
    }
}