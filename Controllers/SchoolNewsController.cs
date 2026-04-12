using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObreshkovLibrary.Data;
using ObreshkovLibrary.Models;
using ObreshkovLibrary.Models.ViewModels;

namespace ObreshkovLibrary.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SchoolNewsController : Controller
    {
        private readonly ObreshkovLibraryContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<IdentityUser> _userManager;

        public SchoolNewsController(
            ObreshkovLibraryContext context,
            IWebHostEnvironment environment,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        private static bool IsAllowedImageExtension(string extension)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            return allowed.Contains(extension.ToLowerInvariant());
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var extension = Path.GetExtension(imageFile.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !IsAllowedImageExtension(extension))
                return null;

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "school-news"
            );

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);

            await imageFile.CopyToAsync(stream);

            return $"/uploads/school-news/{fileName}";
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) ||
                !imagePath.StartsWith("/uploads/school-news/"))
                return;

            var relativePath = imagePath
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            var absolutePath = Path.Combine(
                _environment.WebRootPath,
                relativePath
            );

            if (System.IO.File.Exists(absolutePath))
                System.IO.File.Delete(absolutePath);
        }

        // ================= INDEX =================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var news = await _context.SchoolNews
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenByDescending(x => x.PublishedOn)
                .ThenByDescending(x => x.CreatedOn)
                .ToListAsync();

            return View(news);
        }

        // ================= CREATE =================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new SchoolNewsFormVM
            {
                PublishedOn = DateTime.Today,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchoolNewsFormVM vm)
        {
            if (vm.ImageFile == null)
            {
                ModelState.AddModelError(
                    nameof(vm.ImageFile),
                    "Снимката е задължителна."
                );
            }

            if (!ModelState.IsValid)
                return View(vm);

            var imagePath = await SaveImageAsync(vm.ImageFile);

            var entity = new SchoolNews
            {
                Title = vm.Title.Trim(),
                Summary = vm.Summary.Trim(),
                ImagePath = imagePath,
                NewsUrl = vm.NewsUrl.Trim(),
                PublishedOn = vm.PublishedOn,
                IsActive = vm.IsActive,
                CreatedOn = DateTime.Now
            };

            _context.SchoolNews.Add(entity);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Новината е добавена успешно.";

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT =================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.SchoolNews
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound();

            var vm = new SchoolNewsFormVM
            {
                Id = entity.Id,
                Title = entity.Title,
                Summary = entity.Summary,
                CurrentImagePath = entity.ImagePath,
                NewsUrl = entity.NewsUrl,
                PublishedOn = entity.PublishedOn,
                IsActive = entity.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SchoolNewsFormVM vm)
        {
            if (id != vm.Id)
                return NotFound();

            var entity = await _context.SchoolNews
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                vm.CurrentImagePath = entity.ImagePath;
                return View(vm);
            }

            var oldImagePath = entity.ImagePath;

            var newImagePath = await SaveImageAsync(vm.ImageFile);

            entity.Title = vm.Title.Trim();
            entity.Summary = vm.Summary.Trim();
            entity.NewsUrl = vm.NewsUrl.Trim();
            entity.PublishedOn = vm.PublishedOn;
            entity.IsActive = vm.IsActive;

            if (!string.IsNullOrWhiteSpace(newImagePath))
            {
                entity.ImagePath = newImagePath;
                DeleteImage(oldImagePath);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Новината е редактирана успешно.";

            return RedirectToAction(nameof(Index));
        }

        // ================= ARCHIVE =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id, string password)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var validPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!validPassword)
            {
                TempData["Error"] = "Грешна парола.";
                return RedirectToAction(nameof(Index));
            }

            var entity = await _context.SchoolNews
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound();

            entity.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Новината е архивирана успешно.";

            return RedirectToAction(nameof(Index));
        }
    }
}