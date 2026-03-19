using Microsoft.AspNetCore.Mvc;

namespace ObreshkovLibrary.Controllers
{
    public class CatalogController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}