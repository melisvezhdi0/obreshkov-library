using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ArchiveController : Controller
    {
        private readonly ObreshkovLibraryContext _context;

        public ArchiveController(ObreshkovLibraryContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            search = (search ?? "").Trim();
            var s = search.ToLower();

            var booksQ = _context.Books
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b => !b.IsActive)
                .AsQueryable();

            var clientsQ = _context.Clients
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsActive)
                .AsQueryable();

            var categoriesQ = _context.Categories
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .Where(c => !c.IsActive)
                .AsQueryable();

            var copiesQ = _context.BookCopies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(c => c.Book)
                .Where(c => !c.IsActive)
                .AsQueryable();

            var loansQ = _context.Loans
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .Where(l => l.ReturnDate != null)
                .AsQueryable();

            var newsQ = _context.SchoolNews
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(n => !n.IsActive)
                .AsQueryable();

            var passwordRequestsQ = _context.PasswordResetRequests
               .IgnoreQueryFilters()
               .AsNoTracking()
               .Include(r => r.Client)
               .Where(r => r.IsCompleted)
               .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var compact = s.Replace(" ", "").Replace("-", "");

                booksQ = booksQ.Where(b =>
                    (b.Title ?? "").ToLower().Contains(s) ||
                    (b.Author ?? "").ToLower().Contains(s) ||
                    (b.Description ?? "").ToLower().Contains(s) ||
                    (b.SchoolClass ?? "").ToLower().Contains(s) ||
                    (b.SearchKeywords ?? "").ToLower().Contains(s) ||
                    (b.Category != null && b.Category.Name.ToLower().Contains(s)));

                clientsQ = clientsQ.Where(c =>
                    (((c.FirstName ?? "") + " " + (c.MiddleName ?? "") + " " + (c.LastName ?? "")).ToLower().Contains(s)) ||
                    (c.PhoneNumber ?? "").ToLower().Contains(s) ||
                    (c.CardNumber ?? "").ToLower().Contains(s) ||
                    ((c.CardNumber ?? "").Replace(" ", "").Replace("-", "").ToLower().Contains(compact)));

                categoriesQ = categoriesQ.Where(c =>
                    (c.Name ?? "").ToLower().Contains(s) ||
                    (c.ParentCategory != null && (c.ParentCategory.Name ?? "").ToLower().Contains(s)));

                copiesQ = copiesQ.Where(c =>
                    c.Id.ToString().Contains(search) ||
                    (c.Book.Title ?? "").ToLower().Contains(s) ||
                    (c.Book.Author ?? "").ToLower().Contains(s));

                loansQ = loansQ.Where(l =>
                    (((l.Client.FirstName ?? "") + " " + (l.Client.LastName ?? "")).ToLower().Contains(s)) ||
                    (l.Client.CardNumber ?? "").ToLower().Contains(s) ||
                    ((l.BookCopy.Book.Title ?? "").ToLower().Contains(s)) ||
                    ((l.BookCopy.Book.Author ?? "").ToLower().Contains(s)));

                newsQ = newsQ.Where(n =>
                    (n.Title ?? "").ToLower().Contains(s) ||
                    (n.Summary ?? "").ToLower().Contains(s));

                passwordRequestsQ = passwordRequestsQ.Where(r =>
                    (((r.Client != null ? r.Client.FirstName : "") ?? "") + " " +
                     ((r.Client != null ? r.Client.LastName : "") ?? "")).ToLower().Contains(s) ||
                    (r.CardNumber ?? "").ToLower().Contains(s) ||
                    (r.PhoneNumber ?? "").ToLower().Contains(s) ||
                    (r.Notes ?? "").ToLower().Contains(s));
            }

            var vm = new ArchiveDashboardVM
            {
                Search = search,

                BooksCount = await booksQ.CountAsync(),
                ClientsCount = await clientsQ.CountAsync(),
                CategoriesCount = await categoriesQ.CountAsync(),
                CopiesCount = await copiesQ.CountAsync(),
                LoansCount = await loansQ.CountAsync(),
                SchoolNewsCount = await newsQ.CountAsync(),
                PasswordRequestsCount = await passwordRequestsQ.CountAsync(),

                RecentBooks = await booksQ
                    .OrderByDescending(b => b.Id)
                    .Take(5)
                    .ToListAsync(),

                RecentClients = await clientsQ
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .ToListAsync(),

                RecentCategories = await categoriesQ
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .ToListAsync(),

                RecentCopies = await copiesQ
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .ToListAsync(),

                RecentLoans = await loansQ
                    .OrderByDescending(l => l.ReturnDate)
                    .ThenByDescending(l => l.Id)
                    .Take(5)
                    .ToListAsync(),

                RecentSchoolNews = await newsQ
                    .OrderByDescending(n => n.CreatedOn)
                    .Take(5)
                    .ToListAsync(),

                RecentPasswordRequests = await passwordRequestsQ
                    .OrderByDescending(r => r.RequestedOn)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Books(string? search)
        {
            search = (search ?? "").Trim();
            var q = _context.Books
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(b => !b.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();

                q = q.Where(b =>
                    (b.Title ?? "").ToLower().Contains(s) ||
                    (b.Author ?? "").ToLower().Contains(s) ||
                    (b.Description ?? "").ToLower().Contains(s) ||
                    (b.SchoolClass ?? "").ToLower().Contains(s) ||
                    (b.SearchKeywords ?? "").ToLower().Contains(s) ||
                    (b.Category != null && (b.Category.Name ?? "").ToLower().Contains(s)) ||
                    (b.Category != null && b.Category.ParentCategory != null && (b.Category.ParentCategory.Name ?? "").ToLower().Contains(s)));
            }

            ViewBag.Search = search;

            var items = await q
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Author)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Clients(string? search, string? classFilter)
        {
            search = (search ?? "").Trim();
            classFilter = (classFilter ?? "").Trim();

            var q = _context.Clients
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(classFilter))
            {
                var cf = classFilter.Replace(" ", "").ToUpper();

                q = q.Where(c =>
                    (((c.Grade != null ? c.Grade.ToString() : "") + (c.Section ?? ""))
                        .Replace(" ", "")
                        .ToUpper()) == cf);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                var compact = s.Replace(" ", "").Replace("-", "");

                q = q.Where(c =>
                    (((c.FirstName ?? "") + " " + (c.MiddleName ?? "") + " " + (c.LastName ?? "")).ToLower().Contains(s)) ||
                    (c.PhoneNumber ?? "").ToLower().Contains(s) ||
                    (c.CardNumber ?? "").ToLower().Contains(s) ||
                    ((c.CardNumber ?? "").Replace(" ", "").Replace("-", "").ToLower().Contains(compact)));
            }

            var allArchived = await _context.Clients
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => !c.IsActive)
                .ToListAsync();

            var availableClasses = allArchived
                .Where(c => c.Grade.HasValue)
                .Select(c =>
                {
                    var grade = c.Grade.Value.ToString();
                    var section = (c.Section ?? "").Trim();
                    return string.IsNullOrWhiteSpace(section) ? grade : $"{grade}{section}".Replace(" ", "").ToUpper();
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            ViewBag.Search = search;
            ViewBag.ClassFilter = classFilter;
            ViewBag.AvailableClasses = availableClasses;

            var items = await q
                .OrderBy(c => c.Grade ?? int.MaxValue)
                .ThenBy(c => c.Section)
                .ThenBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Categories(string? search)
        {
            search = (search ?? "").Trim();

            var q = _context.Categories
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(c => c.ParentCategory)
                .Where(c => !c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();

                q = q.Where(c =>
                    (c.Name ?? "").ToLower().Contains(s) ||
                    (c.ParentCategory != null && (c.ParentCategory.Name ?? "").ToLower().Contains(s)));
            }

            ViewBag.Search = search;

            var items = await q
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Copies(string? search)
        {
            search = (search ?? "").Trim();

            var q = _context.BookCopies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(c => c.Book)
                    .ThenInclude(b => b.Category)
                .Where(c => !c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();

                q = q.Where(c =>
                    c.Id.ToString().Contains(search) ||
                    (c.Book.Title ?? "").ToLower().Contains(s) ||
                    (c.Book.Author ?? "").ToLower().Contains(s) ||
                    (c.Book.Category != null && (c.Book.Category.Name ?? "").ToLower().Contains(s)));
            }

            ViewBag.Search = search;

            var items = await q
                .OrderBy(c => c.Book.Title)
                .ThenBy(c => c.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Loans(string? search)
        {
            search = (search ?? "").Trim();

            var q = _context.Loans
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .Where(l => l.ReturnDate != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();

                q = q.Where(l =>
                    (((l.Client.FirstName ?? "") + " " + (l.Client.MiddleName ?? "") + " " + (l.Client.LastName ?? "")).ToLower().Contains(s)) ||
                    (l.Client.CardNumber ?? "").ToLower().Contains(s) ||
                    (l.BookCopy.Book.Title ?? "").ToLower().Contains(s) ||
                    (l.BookCopy.Book.Author ?? "").ToLower().Contains(s));
            }

            ViewBag.Search = search;

            var items = await q
                .OrderByDescending(l => l.ReturnDate)
                .ThenByDescending(l => l.Id)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> SchoolNews(string? search)
        {
            search = (search ?? "").Trim();

            var q = _context.SchoolNews
              .IgnoreQueryFilters()
              .AsNoTracking()
              .Where(n => !n.IsActive)
              .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();

                q = q.Where(n =>
                    (n.Title ?? "").ToLower().Contains(s) ||
                    (n.Summary ?? "").ToLower().Contains(s));
            }

            ViewBag.Search = search;

            var items = await q
                .OrderByDescending(n => n.CreatedOn)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> PasswordRequests(string? search)
        {
            search = (search ?? "").Trim();

            var q = _context.PasswordResetRequests
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(r => r.Client)
                .Where(r => r.IsCompleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                var compact = s.Replace(" ", "").Replace("-", "");

                q = q.Where(r =>
                    (((r.Client != null ? r.Client.FirstName : "") ?? "") + " " +
                     ((r.Client != null ? r.Client.MiddleName : "") ?? "") + " " +
                     ((r.Client != null ? r.Client.LastName : "") ?? "")).ToLower().Contains(s) ||
                    (r.CardNumber ?? "").ToLower().Contains(s) ||
                    (r.PhoneNumber ?? "").ToLower().Contains(s) ||
                    ((r.CardNumber ?? "").Replace(" ", "").Replace("-", "").ToLower().Contains(compact)) ||
                    (r.Notes ?? "").ToLower().Contains(s));
            }

            ViewBag.Search = search;

            var items = await q
                .OrderByDescending(r => r.RequestedOn)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreBook(int id)
        {
            var book = await _context.Books
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            book.IsActive = true;

            var copies = await _context.BookCopies
                .IgnoreQueryFilters()
                .Where(c => c.BookId == id)
                .ToListAsync();

            foreach (var copy in copies)
                copy.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Книгата беше възстановена успешно.";

            return RedirectToAction(nameof(Books));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreClient(int id)
        {
            var client = await _context.Clients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return NotFound();

            client.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Читателят беше възстановен успешно.";

            return RedirectToAction(nameof(Clients));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            var category = await _context.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            var parentId = category.ParentCategoryId;

            while (parentId != null)
            {
                var parent = await _context.Categories
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == parentId.Value);

                if (parent == null)
                    break;

                if (!parent.IsActive)
                    parent.IsActive = true;

                parentId = parent.ParentCategoryId;
            }

            category.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Категорията беше възстановена успешно.";

            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreCopy(int id)
        {
            var copy = await _context.BookCopies
                .IgnoreQueryFilters()
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (copy == null)
                return NotFound();

            if (!copy.Book.IsActive)
            {
                TempData["ArchiveError"] = "Копието не може да бъде възстановено, защото книгата е архивирана.";
                return RedirectToAction(nameof(Copies));
            }

            copy.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Копието беше възстановено успешно.";

            return RedirectToAction(nameof(Copies));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreLoan(int id)
        {
            var loan = await _context.Loans
                .IgnoreQueryFilters()
                .Include(l => l.Client)
                .Include(l => l.BookCopy)
                    .ThenInclude(bc => bc.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
                return NotFound();

            if (loan.ReturnDate == null)
            {
                TempData["ArchiveError"] = "Този заем вече е активен.";
                return RedirectToAction(nameof(Loans));
            }

            if (!loan.Client.IsActive || !loan.BookCopy.IsActive || !loan.BookCopy.Book.IsActive)
            {
                TempData["ArchiveError"] = "Заемът не може да бъде възстановен, защото читателят, книгата или копието са архивирани.";
                return RedirectToAction(nameof(Loans));
            }

            bool hasOpenLoanForCopy = await _context.Loans
                .IgnoreQueryFilters()
                .AnyAsync(l => l.Id != loan.Id && l.BookCopyId == loan.BookCopyId && l.ReturnDate == null);

            if (hasOpenLoanForCopy)
            {
                TempData["ArchiveError"] = "Това копие вече е заето.";
                return RedirectToAction(nameof(Loans));
            }

            loan.ReturnDate = null;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Заемът беше възстановен успешно.";

            return RedirectToAction(nameof(Loans));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreSchoolNews(int id)
        {
            var news = await _context.SchoolNews
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
                return NotFound();

            news.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Публикацията беше възстановена успешно.";

            return RedirectToAction(nameof(SchoolNews));
        }
    }
}