using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models.ViewModels;

[Authorize(Roles = "Student")]
public class StudentPortalController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ObreshkovLibraryContext _context;

    public StudentPortalController(UserManager<IdentityUser> userManager, ObreshkovLibraryContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var cardNumber = user.UserName?.Trim().ToUpper();
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return Challenge();
        }

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.CardNumber != null && c.CardNumber.ToUpper() == cardNumber);

        if (client == null)
        {
            return Challenge();
        }

        var currentLoans = await _context.Loans
            .Where(l => l.ClientId == client.Id && l.ReturnDate == null)
            .Include(l => l.BookCopy)
                .ThenInclude(bc => bc.Book)
            .OrderByDescending(l => l.LoanDate)
            .Take(5)
            .ToListAsync();

        var vm = new StudentDashboardVM
        {
            StudentName = $"{client.FirstName} {client.LastName}".Trim(),
            CurrentLoansCount = await _context.Loans.CountAsync(l => l.ClientId == client.Id && l.ReturnDate == null),
            FavoritesCount = await _context.ClientFavoriteBooks.CountAsync(f => f.ClientId == client.Id),
            UnreadNotificationsCount = await _context.StudentNotifications.CountAsync(n => n.ClientId == client.Id && !n.IsRead),
            CurrentLoans = currentLoans.Select(l => new StudentCurrentLoanVM
            {
                BookId = l.BookCopy.Book.Id,
                Title = l.BookCopy.Book.Title,
                Author = l.BookCopy.Book.Author,
                LoanDate = l.LoanDate,
                DueDate = l.DueDate,
                IsOverdue = l.DueDate.Date < DateTime.Today,
                IsExtended = l.IsExtended
            }).ToList()
        };

        ViewBag.RequirePasswordChange = !client.PasswordChangedByStudent;

        return View(vm);
    }
}