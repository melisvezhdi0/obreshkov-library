using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObreshkovLibrary.Services;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentNotificationsAdminController : Controller
    {
        private readonly IStudentNotificationService _notificationService;

        public StudentNotificationsAdminController(IStudentNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAdminMessage(string title, string message)
        {
            title = (title ?? string.Empty).Trim();
            message = (message ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Заглавието и съобщението са задължителни.";
                return RedirectToAction(nameof(Index));
            }

            await _notificationService.SendAdminNotificationToAllAsync(title, message);

            TempData["SuccessMessage"] = "Административното съобщение е изпратено.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunLoanReminders()
        {
            await _notificationService.ProcessLoanDueRemindersAsync();
            TempData["SuccessMessage"] = "Проверката за напомнянията е изпълнена.";
            return RedirectToAction(nameof(Index));
        }
    }
}