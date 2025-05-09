using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.Service.Chapter;
using PBL3.Service.Story;
using PBL3.ViewModels.Story;

namespace PBL3.Controllers
{
    public class StoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IChapterService _chapterService;
        private readonly IStoryService _storyService;
        public StoryController(ApplicationDbContext context, BlobService blobService, IChapterService chapterService, IStoryService storyService)
        {
            _context = context;
            _blobService = blobService;
            _chapterService = chapterService;
            _storyService = storyService;
        }

        // GET: Story/View/{id}
        public async Task<IActionResult> View(int id)
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var storyDetail = await _storyService.GetStoryDetailAsync(id, currentUserID);
            if (storyDetail == null)
            {
                return NotFound();
            }

            return View(storyDetail);
        }



        // GET: Story/Create
        public IActionResult Create()
        {
            var model = new StoryCreateViewModel
            {
                availbleGenres = GetAvailbleGenres()
            };
            return View(model);
        }

        // POST: Story/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.availbleGenres = GetAvailbleGenres();
                return View(model);
            }

            int authorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var (isSuccess, errorMessage, storyID) = await _storyService.CreateStoryAsync(model, authorID);

            if (!isSuccess)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                model.availbleGenres = GetAvailbleGenres();
                return View(model);
            }

            TempData["SuccessMessage"] = "Tạo truyện mới thành công!";
            return RedirectToAction("MyStories", "User");
        }

        //POST: Story/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int StoryID)
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _storyService.DeleteStoryAsync(StoryID, currentUserID);

            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("MyStories", "User");
            }
            TempData["SuccessMessage"] = "Xóa truyện thành công!";
            return RedirectToAction("MyStories", "User");
        }

        //POST: Story/UpdateStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int StoryID, string newStatus)
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var (isSuccess, errorMessage, storyID) = await _storyService.UpdateStoryStatusAsync(StoryID, currentUserID, newStatus);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("MyStories", "User");
            }
            TempData["SuccessMessage"] = "Cập nhật trạng thái truyện thành công!";
            return RedirectToAction("EditDetail", new { id = storyID });
        }


        // GET: Story/EditDetail/{id}
        public async Task<IActionResult> EditDetail(int id)
        {
            int currentAuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var viewModel = await _storyService.GetStoryDetailForEditAsync(id, currentAuthorID);

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        //GET: Story/Edit
        public async Task<IActionResult> Edit(int id)
        {
            int currentAuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var viewModel = await _storyService.GetStoryDetailForEditAsync(id, currentAuthorID);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        //POST: Story/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StoryEditViewModel model, int currentUserID)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            int currentAuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _storyService.UpdateStoryAsync(model, currentAuthorID);
            if (!isSuccess)
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }
            TempData["SuccessMessage"] = "Cập nhật truyện thành công!";
            return RedirectToAction("MyStories", "User");
        }

        //METHOD
        private List<GerneVM> GetAvailbleGenres()
        {
            return _context.Genres
                .Select(g => new GerneVM
                {
                    GenreID = g.GenreID,
                    Name = g.Name
                })
                .ToList();
        }

    }
}
