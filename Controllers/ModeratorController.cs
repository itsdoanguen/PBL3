using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Moderator;

namespace PBL3.Controllers
{
    [Authorize(Roles = "Moderator")]
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
    }
}
