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

        // GET: Discovery/TopWeek
        public async Task<IActionResult> TopWeek(int page = 1, int pageSize = 40)
        {
            var allStories = await _dashboardService.GetTopStoriesOfWeekAsync(1000); // Get more than enough
            var totalCount = allStories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Top Truyện Tuần";
            ViewBag.PageDescription = "Bảng xếp hạng những truyện hot nhất tuần này";

            return View("StoriesList", pagedStories);
        }

        // GET: Discovery/MostLiked
        public async Task<IActionResult> MostLiked(int page = 1, int pageSize = 40)
        {
            var allStories = await _dashboardService.GetMostLikedStoriesAsync(1000);
            var totalCount = allStories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Truyện Được Yêu Thích Nhất";
            ViewBag.PageDescription = "Những truyện có lượt thích cao nhất";

            return View("StoriesList", pagedStories);
        }

        // GET: Discovery/Hot
        public async Task<IActionResult> Hot(int page = 1, int pageSize = 40, int categoryId = 0)
        {
            var allStories = await _dashboardService.GetHotStoriesByCategoryAsync(categoryId, 1000);
            var totalCount = allStories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var categories = await _dashboardService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Truyện Hot";
            ViewBag.PageDescription = "Những truyện được đọc nhiều nhất";

            return View("StoriesList", pagedStories);
        }

        // GET: Discovery/New
        public async Task<IActionResult> New(int page = 1, int pageSize = 40)
        {
            var allStories = await _dashboardService.GetNewStoriesAsync(1000);
            var totalCount = allStories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Truyện Mới";
            ViewBag.PageDescription = "Những truyện mới được đăng gần đây";

            return View("StoriesList", pagedStories);
        }

        // GET: Discovery/Completed
        public async Task<IActionResult> Completed(int page = 1, int pageSize = 40)
        {
            var allStories = await _dashboardService.GetCompletedStoriesAsync(1000);
            var totalCount = allStories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Truyện Hoàn Thành";
            ViewBag.PageDescription = "Những truyện đã hoàn thành";

            return View("StoriesList", pagedStories);
        }

        // GET: Discovery/FollowedStories (requires login)
        public async Task<IActionResult> FollowedStories(int page = 1, int pageSize = 40)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var allStories = await _dashboardService.GetFollowedStoriesAsync(userId, 1000);
            var totalCount = allStories.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var pagedStories = allStories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageTitle = "Truyện Đang Theo Dõi";
            ViewBag.PageDescription = "Những truyện bạn đang theo dõi";

            return View("StoriesList", pagedStories);
        }

        // GET: Discovery/FollowedAuthors (requires login)
        public async Task<IActionResult> FollowedAuthors(int page = 1, int pageSize = 24)
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
            ViewBag.PageTitle = "Tác Giả Đang Theo Dõi";
            ViewBag.PageDescription = "Những tác giả bạn đang theo dõi";

            return View("AuthorsList", pagedAuthors);
        }
    }
} 