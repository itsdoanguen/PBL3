using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Story;

namespace PBL3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IStoryService _storyService;
        public AdminController(IStoryService storyService)
        {
            _storyService = storyService;
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
    }
}
