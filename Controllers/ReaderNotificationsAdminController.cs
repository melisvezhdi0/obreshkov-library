using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObreshkovLibrary.Services;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReaderNotificationsAdminController : Controller
    {
        private readonly IReaderNotificationService _notificationService;

        public ReaderNotificationsAdminController(IReaderNotificationService notificationService)
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
        public async Task<IActionResult> RunLoanReminders()
        {
            await _notificationService.ProcessLoanDueRemindersAsync();
            TempData["SuccessMessage"] = "Проверката за напомнянията е изпълнена.";
            return RedirectToAction(nameof(Index));
        }
    }
}