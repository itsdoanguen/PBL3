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
            ViewBag.LayoutPath = GetLayoutForCurrentUser();
            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return View("PageNotFound");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.LayoutPath = GetLayoutForCurrentUser();
            ViewBag.UserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (statusCode == 404)
            {
                return RedirectToAction("PageNotFound");
            }

            return View("Error");
        }

        private string GetLayoutForCurrentUser()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (User.Identity.IsAuthenticated)
            {
                return userRole switch
                {
                    "Admin" => "~/Views/Shared/_AdminLayout.cshtml",
                    "User" => "~/Views/Shared/_UserLayout.cshtml",
                    _ => "~/Views/Shared/_ModeratorLayout.cshtml"
                };
            }

            return "~/Views/Shared/_AuthenticationLayout.cshtml";
        }

    }
}
