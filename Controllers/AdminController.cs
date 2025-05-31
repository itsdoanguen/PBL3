using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Story;
using PBL3.Service.Admin;

namespace PBL3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IStoryService _storyService;
        private readonly IAdminService _adminService;
        public AdminController(IStoryService storyService, IAdminService adminService)
        {
            _storyService = storyService;
            _adminService = adminService;
        }
        // GET: Admin/Index
        public IActionResult Index()
        {
            return View();
        }
        //POST: Admin/DeleteStory/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStory(int StoryID)
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _storyService.DeleteStoryAsync(StoryID, currentUserID);

            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("StoryManagement", "Moderator");
            }
            TempData["SuccessMessage"] = "Xóa truyện thành công!";
            return RedirectToAction("StoryManagement", "Moderator");
        }
        //GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = await _adminService.GetDashboardStatsAsync();
            return View(model);
        }
        // GET: Admin/Report
        public async Task<IActionResult> Report(DateTime? from, DateTime? to, int? tagId)
        {
            from ??= DateTime.Now.Date.AddDays(-7);
            to ??= DateTime.Now.Date.AddDays(1).AddTicks(-1);
            var model = await _adminService.GetReportStatsAsync(from, to, tagId);
            return View(model);
        }
        //GET: Admin/ManageSystem
        public async Task<IActionResult> ManageSystem()
        {
            var model = await _adminService.GetManageSystemStatsAsync();
            return View(model);
        }
    }
}
