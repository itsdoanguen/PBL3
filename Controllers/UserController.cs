using Microsoft.AspNetCore.Mvc;

namespace PBL3.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
