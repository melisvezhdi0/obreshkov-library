#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;

namespace ObreshkovLibrary.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ObreshkovLibraryContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ObreshkovLibraryContext context,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Display(Name = "Имейл")]
            public string? Email { get; set; }

            [Display(Name = "Номер на читателска карта")]
            public string? CardNumber { get; set; }

            [Required(ErrorMessage = "Полето за парола е задължително.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Запомни ме")]
            public bool RememberMe { get; set; }

            [Required(ErrorMessage = "Избери роля.")]
            public string SelectedRole { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (string.IsNullOrWhiteSpace(Input.SelectedRole))
            {
                ModelState.AddModelError(string.Empty, "Избери роля.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Полето за парола е задължително.");
                return Page();
            }

            if (Input.SelectedRole == "Admin")
            {
                if (string.IsNullOrWhiteSpace(Input.Email))
                {
                    ModelState.AddModelError(string.Empty, "Въведи имейл.");
                    return Page();
                }

                var adminUser = await _userManager.FindByEmailAsync(Input.Email);
                if (adminUser == null || !await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    ModelState.AddModelError(string.Empty, "Невалиден администраторски вход.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(
                    adminUser.UserName,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin logged in.");
                    return LocalRedirect(Url.Action("Index", "Dashboard") ?? "/Dashboard");
                }

                ModelState.AddModelError(string.Empty, "Невалиден опит за вход.");
                return Page();
            }

            if (Input.SelectedRole == "Student")
            {
                if (string.IsNullOrWhiteSpace(Input.CardNumber))
                {
                    ModelState.AddModelError(string.Empty, "Въведи номер на читателска карта.");
                    return Page();
                }

                var normalizedCardNumber = Input.CardNumber.Trim().ToUpper();

                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.CardNumber.ToUpper() == normalizedCardNumber);

                if (client == null)
                {
                    ModelState.AddModelError(string.Empty, "Няма активен ученик с такава читателска карта.");
                    return Page();
                }

                var studentUser = await _userManager.FindByNameAsync(normalizedCardNumber);
                if (studentUser == null || !await _userManager.IsInRoleAsync(studentUser, "Student"))
                {
                    ModelState.AddModelError(string.Empty, "Няма ученически профил за тази карта.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(
                    studentUser.UserName,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Student logged in.");
                    return LocalRedirect(Url.Action("Index", "Catalog") ?? "/Catalog");
                }

                ModelState.AddModelError(string.Empty, "Невалиден опит за вход.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Невалидна роля.");
            return Page();
        }
    }
}