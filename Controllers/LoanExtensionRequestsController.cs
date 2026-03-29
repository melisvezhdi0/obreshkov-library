using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Services;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoanExtensionRequestsController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly IStudentNotificationService _notificationService;

        public LoanExtensionRequestsController(
            ObreshkovLibraryContext context,
            IStudentNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.LoanExtensionRequests
                .Include(r => r.Loan)
                    .ThenInclude(l => l.Client)
                .Include(r => r.Loan)
                    .ThenInclude(l => l.BookCopy)
                        .ThenInclude(c => c.Book)
                .OrderBy(r => r.Status)
                .ThenByDescending(r => r.RequestedOn)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.LoanExtensionRequests
                .Include(r => r.Loan)
                    .ThenInclude(l => l.BookCopy)
                        .ThenInclude(c => c.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Заявката не беше намерена.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != LoanExtensionRequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "Само чакащи заявки могат да бъдат одобрени.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Loan.IsExtended)
            {
                TempData["ErrorMessage"] = "Този заем вече е бил удължен.";
                return RedirectToAction(nameof(Index));
            }

            request.Status = LoanExtensionRequestStatus.Approved;
            request.ProcessedOn = DateTime.Now;

            request.Loan.DueDate = request.Loan.DueDate.AddDays(request.RequestedDays);
            request.Loan.IsExtended = true;

            request.Loan.Reminder7DaysSent = false;
            request.Loan.Reminder3DaysSent = false;
            request.Loan.Reminder1DaySent = false;
            request.Loan.LastOverdueReminderSentOn = null;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                request.Loan.ClientId,
                $"Одобрено удължаване: {request.Loan.BookCopy.Book.Title}",
                $"Заявката ти за удължаване на книгата „{request.Loan.BookCopy.Book.Title}“ беше одобрена. Новата дата за връщане е {request.Loan.DueDate:dd.MM.yyyy}.",
                StudentNotificationType.ExtensionRequest,
                request.Loan.BookCopy.BookId,
                request.LoanId);

            TempData["SuccessMessage"] = "Заявката е одобрена.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? adminResponseMessage)
        {
            var request = await _context.LoanExtensionRequests
                .Include(r => r.Loan)
                    .ThenInclude(l => l.BookCopy)
                        .ThenInclude(c => c.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                TempData["ErrorMessage"] = "Заявката не беше намерена.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != LoanExtensionRequestStatus.Pending)
            {
                TempData["ErrorMessage"] = "Само чакащи заявки могат да бъдат отхвърлени.";
                return RedirectToAction(nameof(Index));
            }

            request.Status = LoanExtensionRequestStatus.Rejected;
            request.ProcessedOn = DateTime.Now;
            request.AdminResponseMessage = string.IsNullOrWhiteSpace(adminResponseMessage)
                ? null
                : adminResponseMessage.Trim();

            await _context.SaveChangesAsync();

            var message = $"Заявката ти за удължаване на книгата „{request.Loan.BookCopy.Book.Title}“ беше отхвърлена.";
            if (!string.IsNullOrWhiteSpace(request.AdminResponseMessage))
            {
                message += $" Причина: {request.AdminResponseMessage}";
            }

            await _notificationService.CreateNotificationAsync(
                request.Loan.ClientId,
                $"Отхвърлено удължаване: {request.Loan.BookCopy.Book.Title}",
                message,
                StudentNotificationType.ExtensionRequest,
                request.Loan.BookCopy.BookId,
                request.LoanId);

            TempData["SuccessMessage"] = "Заявката е отхвърлена.";
            return RedirectToAction(nameof(Index));
        }
    }
}