using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Moderator;

namespace PBL3.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class ModeratorController : Controller
    {
        private readonly IModeratorService _moderatorService;
        public ModeratorController(IModeratorService moderatorService)
        {
            _moderatorService = moderatorService;
        }
        //GET: Moderator/Index
        public IActionResult Index()
        {
            return View();
        }
        //GET: Moderator/ReportsManagement
        [HttpGet]
        public IActionResult ReportsManagement()
        {
            return RedirectToAction("Index", "Report");
        }
        //GET: Moderator/UsersManagement
        [HttpGet]
        public async Task<IActionResult> UsersManagement()
        {
            var viewModel = await _moderatorService.GetListUserForModeratorAsync();
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }
        //GET: Moderator/StoryManagement
        [HttpGet]
        public async Task<IActionResult> StoryManagement()
        {
            var viewModel = await _moderatorService.GetListStoriesForModeratorAsync();
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }
        //GET: Moderator/ViewUser/{id}
        [HttpGet]
        public async Task<IActionResult> ViewUser(int id)
        {
            var viewModel = await _moderatorService.GetViewUserViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }
        //GET: Moderator/ViewStory/{id}
        [HttpGet]
        public async Task<IActionResult> ViewStory(int id)
        {
            var viewModel = await _moderatorService.GetViewStoryViewModelAsync(id);
            if (viewModel == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }
        //POST: Moderator/WarnUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WarnUser(int id, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(string.Empty, "Nội dung cảnh cáo không được để trống.");
                return RedirectToAction("ViewUser", new { id });
            }
            int moderatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _moderatorService.WarnUserAsync(id, message, moderatorId);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("ViewUser", new { id });
            }
            TempData["SuccessMessage"] = "Cảnh cáo người dùng thành công!";
            return RedirectToAction("ViewUser", new { id });
        }
        //POST: Moderator/BanUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(int id, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(string.Empty, "Nội dung cấm không được để trống.");
                return RedirectToAction("ViewUser", new { id });
            }
            var moderatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _moderatorService.BanUserAsync(id, message, moderatorId);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("ViewUser", new { id });
            }
            TempData["SuccessMessage"] = "Cấm người dùng thành công!";
            return RedirectToAction("ViewUser", new { id });
        }
        //POST: Moderator/UnbanUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(int id, string message)
        {
            int moderatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _moderatorService.UnbanUserAsync(id, message, moderatorId);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("ViewUser", new { id });
            }
            TempData["SuccessMessage"] = "Mở cấm người dùng thành công!";
            return RedirectToAction("ViewUser", new { id });
        }
    }
}
