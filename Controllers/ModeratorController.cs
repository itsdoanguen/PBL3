using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PBL3.Controllers
{
    [Authorize(Roles = "Moderator")]
    public class ModeratorController : Controller
    {
        //GET: Moderator/Index
        public IActionResult Index()
        {
            return View();
        }
        //GET: Moderator/MyProfile
        public IActionResult MyProfile()
        {
            return RedirectToAction("MyProfile","User");
        }
    }
}
