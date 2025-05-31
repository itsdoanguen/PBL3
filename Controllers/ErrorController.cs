using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace PBL3.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult PageNotFound()
        {
            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return View("PageNotFound");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (statusCode == 404)
            {
                return RedirectToAction("PageNotFound");
            }

            return View("Error");
        }
    }
}
