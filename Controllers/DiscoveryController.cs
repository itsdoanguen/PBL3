using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Dashboard;

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
            ViewBag.PageTitle = "Top Featured Authors";
            ViewBag.PageDescription = "Authors with the highest follower counts";

            return View("AuthorsList", pagedAuthors);
        }

        //GET: Discovery/StoryFromFollowedAuthors
        public async Task<IActionResult> StoryFromFollowedAuthors(int page = 1, int pageSize = 8)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var allStories = await _dashboardService.GetStoriesFromFollowedAuthorsAsync(userId, 1000);
            var totalCount = allStories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Stories from Followed Authors";
            ViewBag.PageDescription = "Latest stories from the authors you follow";

            return View("StoryList", pagedStories);
        }
    }
}