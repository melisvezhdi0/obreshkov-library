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
            ReturnUrl = returnUrl;

            _logger.LogInformation("RememberMe = {RememberMe}", Input.RememberMe);

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

                var email = Input.Email.Trim();
                var adminUser = await _userManager.FindByEmailAsync(email);

                if (adminUser == null || !await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    ModelState.AddModelError(string.Empty, "Невалиден администраторски вход.");
                    return Page();
                }

                var passwordOk = await _userManager.CheckPasswordAsync(adminUser, Input.Password);
                if (!passwordOk)
                {
                    ModelState.AddModelError(string.Empty, "Невалиден опит за вход.");
                    return Page();
                }

                await SignInUserExplicitlyAsync(adminUser, Input.RememberMe);

                _logger.LogInformation("Admin logged in.");
                return LocalRedirect(Url.Action("Index", "Dashboard") ?? "/Dashboard");
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
                    .FirstOrDefaultAsync(c => c.CardNumber != null && c.CardNumber.ToUpper() == normalizedCardNumber);

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

                var passwordOk = await _userManager.CheckPasswordAsync(studentUser, Input.Password);
                if (!passwordOk)
                {
                    ModelState.AddModelError(string.Empty, "Невалиден опит за вход.");
                    return Page();
                }

                await SignInUserExplicitlyAsync(studentUser, Input.RememberMe);

                _logger.LogInformation("Student logged in.");

                if (!client.PasswordChangedByStudent)
                {
                    return LocalRedirect(Url.Action("Index", "StudentPortal") ?? "/StudentPortal");
                }

                return LocalRedirect(Url.Action("Index", "StudentPortal") ?? "/StudentPortal");
            }

            ModelState.AddModelError(string.Empty, "Невалидна роля.");
            return Page();
        }

        private async Task SignInUserExplicitlyAsync(IdentityUser user, bool rememberMe)
        {
            await _signInManager.SignOutAsync();

            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                AllowRefresh = true
            };

            if (rememberMe)
            {
                authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);
            }

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal,
                authProperties);
        }
    }
}