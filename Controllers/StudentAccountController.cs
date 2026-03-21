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

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new StudentChangePasswordVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(StudentChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
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
                    await _context.SaveChangesAsync();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Паролата е сменена успешно.";

            return RedirectToAction(nameof(ChangePassword));
        }
    }
}