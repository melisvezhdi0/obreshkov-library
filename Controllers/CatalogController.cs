using Microsoft.AspNetCore.Mvc;
using ObreshkovLibrary.Services.Interfaces;

namespace ObreshkovLibrary.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId, string? sort)
        {
            var vm = await _catalogService.BuildCatalogIndexAsync(search, categoryId, sort);
            ViewBag.FavoriteBookIds = await _catalogService.GetFavoriteBookIdsAsync(User);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> ReaderIndex(string? search, int? categoryId, string? sort)
        {
            var vm = await _catalogService.BuildCatalogIndexAsync(search, categoryId, sort);
            ViewBag.FavoriteBookIds = await _catalogService.GetFavoriteBookIdsAsync(User);
            return View("ReaderIndex", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var book = await _catalogService.BuildDetailsBookAsync(id);
            if (book == null)
                return NotFound();

            await _catalogService.FillDetailsViewBagsAsync(book, User, ViewBag);
            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> ReaderDetails(int id)
        {
            if (!(User.Identity?.IsAuthenticated == true && User.IsInRole("Reader")))
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var book = await _catalogService.BuildDetailsBookAsync(id);
            if (book == null)
                return NotFound();

            await _catalogService.FillDetailsViewBagsAsync(book, User, ViewBag);
            return View(book);
        }
    }
}