using System.Security.Claims;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.Service.Chapter;
using PBL3.Service.Comment;
using PBL3.Service.History;
using PBL3.Service.Notification;
using PBL3.ViewModels.Chapter;

namespace PBL3.Controllers
{
    [Authorize]
    public class ChapterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IChapterService _chapterService;
        private readonly ICommentService _commentService;
        private readonly IHistoryService _historyService;
        public ChapterController(ApplicationDbContext context, IChapterService chapterService, ICommentService commentService, IHistoryService historyService)
        {
            _context = context;
            _chapterService = chapterService;
            _commentService = commentService;
            _historyService = historyService;
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

            if (int.TryParse(currentUserId, out int userId))
            {
                await _historyService.UpdateHistoryAsync(userId, viewModel.StoryID, id);
            }
            ViewBag.AuthorID = _context.Stories
                .Where(s => s.StoryID == viewModel.StoryID)
                .Select(s => s.AuthorID)
                .FirstOrDefault();
            ViewBag.IsModerator = User.IsInRole("Moderator") || User.IsInRole("Admin");
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

        [HttpGet]
        public async Task<IActionResult> GetCommentsJson(string type, int id)
        {
            var comments = await _commentService.GetCommentsAsync(type, id);
            return Json(comments);
        }

        // Lấy 2 tầng đầu 
        [HttpGet]
        public async Task<IActionResult> GetRootAndFirstLevelReplies(string type, int id)
        {
            var comments = await _commentService.GetRootAndFirstLevelRepliesAsync(type, id);
            return Json(comments);
        }

        // Lấy replies theo parentId
        [HttpGet]
        public async Task<IActionResult> GetReplies(int parentCommentId, int? chapterId, int? storyId)
        {
            string type;
            int id;
            if (chapterId.HasValue)
            {
                type = "chapter";
                id = chapterId.Value;
            }
            else if (storyId.HasValue)
            {
                type = "story";
                id = storyId.Value;
            }
            else
            {
                return BadRequest("Missing chapterId or storyId");
            }

            var replies = await _commentService.GetRepliesAsync(type, id, parentCommentId);
            return PartialView("_RepliesPartial", replies);
        }
    }
}
