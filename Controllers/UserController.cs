using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PBL3.Controllers
{
    [Authorize(Roles ="User")]
    public class UserController : Controller
    {
        public IActionResult Index()
        {
           return View();
        }
    }
}
