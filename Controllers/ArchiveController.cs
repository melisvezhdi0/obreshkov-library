using Microsoft.AspNetCore.Mvc;

namespace ObreshkovLibrary.Controllers
{
    public class ArchiveController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}