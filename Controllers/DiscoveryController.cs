using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Dashboard;
using System.Security.Claims;

namespace PBL3.Controllers
{
    public class DiscoveryController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DiscoveryController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        
        // GET: Discovery/FollowedAuthors
        public async Task<IActionResult> FollowedAuthors(int page = 1, int pageSize = 8)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var allAuthors = await _dashboardService.GetFollowedAuthorsAsync(userId, 1000);
            var totalCount = allAuthors.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedAuthors = allAuthors
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Top Tác giả nổi bật";
            ViewBag.PageDescription = "Những tác giả có lượt theo dõi cao";

            return View("AuthorsList", pagedAuthors);
        }
    }
} 