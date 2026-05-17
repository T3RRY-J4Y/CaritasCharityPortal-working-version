using Microsoft.AspNetCore.Mvc;

namespace CaritasCharityPortal.Controllers
{
    public class UserController : Controller
    {
        public IActionResult AddRequest
            ()
        {
            return View();
        }

        public IActionResult RequestAssistance()
        {
            return View();
        }
    }
}
