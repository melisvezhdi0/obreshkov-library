using Microsoft.AspNetCore.Mvc;

namespace ObreshkovLibrary.Controllers
{
    public class GateController : Controller
    {
        private readonly IConfiguration _config;
        public GateController(IConfiguration config) => _config = config;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Unlock([FromForm] string password)
        {
            var correct = _config["AppGate:Password"];

            if (string.IsNullOrWhiteSpace(password) || password != correct)
                return Unauthorized();

            HttpContext.Session.SetString("GateOk", "1");
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Lock()
        {
            HttpContext.Session.Remove("GateOk");
            return Ok();
        }
    }
}