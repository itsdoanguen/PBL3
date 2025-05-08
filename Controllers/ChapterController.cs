using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.Service;
using PBL3.ViewModels.Chapter;

namespace PBL3.Controllers
{
    public class ChapterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IChapterService _chapterService;
        public ChapterController(ApplicationDbContext context, IChapterService chapterService)
        {
            _context = context;
            _chapterService = chapterService;
        }
        public IActionResult Index()
        {
            return View();
        }
        // GET: Chapter/ReadChapter/{id}
        public async Task<IActionResult> ReadChapter(int id)
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var viewModel = await _chapterService.GetChapterDetailAsync(
                id,
                currentUserId,
                cookieName => Request.Cookies.ContainsKey(cookieName),
                (cookieName, value, options) => Response.Cookies.Append(cookieName, value, options)
            );

            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

        // POST: Chapter/CreateChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChapter(ChapterCreateViewModel chapter)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Có lỗi xảy ra trong quá trình tạo chương mới. Vui lòng thử lại.";
                return RedirectToAction("EditDetail", "Story", new { id = chapter.StoryID });
            }

            var newChapter = await _chapterService.CreateChapterAsync(chapter);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    chapter = new
                    {
                        ChapterID = newChapter.ChapterID,
                        Title = newChapter.Title,
                        CreatedAt = newChapter.CreatedAt.ToString("o"),
                        UpdatedAt = newChapter.UpdatedAt?.ToString("o"),
                        ViewCount = newChapter.ViewCount
                    }
                });
            }

            return RedirectToAction("EditDetail", "Story", new { id = chapter.StoryID });
        }


        // POST: Chapter/DeleteChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChapter(int chapterID, int storyID)
        {
            await _chapterService.DeleteChapterAsync(chapterID, storyID);

            // Nếu là Ajax request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("EditDetail", "Story", new { id = storyID });
        }


        // GET: Chapter/EditChapter/{id}
        public async Task<IActionResult> EditChapter(int chapterId, int storyId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var viewModel = await _chapterService.GetChapterForEditAsync(chapterId, storyId, currentUserId);

            if (viewModel == null)
            {
                return RedirectToAction("AccessDenied", "Authentication");
            }

            return View(viewModel);
        }

        // POST: Chapter/EditChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(ChapterEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình cập nhật chương. Vui lòng thử lại.";
                return View(model);
            }

            var success = await _chapterService.UpdateChapterAsync(model);

            if (!success)
            {
                TempData["ErrorMessage"] = "Chương không tồn tại.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Cập nhật chương thành công.";
            return RedirectToAction("EditChapter", new { chapterId = model.ChapterID, storyId = model.StoryID });
        }


        // POST: Chapter/UpdateChapterStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateChapterStatus(int chapterId, string newStatus)
        {
            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _chapterService.UpdateChapterStatusAsync(chapterId, currentUserId, newStatus);

            if (!result.Success)
            {
                if (result.Message == "AccessDenied")
                {
                    return RedirectToAction("AccessDenied", "Authentication");
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("EditChapter", new { chapterId = chapterId, storyId = result.StoryId });
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction("EditChapter", new { chapterId = chapterId, storyId = result.StoryId });
        }

    }
}
