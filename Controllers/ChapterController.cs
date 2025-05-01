    using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Chapter;

namespace PBL3.Controllers
{
    public class ChapterController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ChapterController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        // GET: Chapter/ReadChapter/{id}
        public async Task<IActionResult> ReadChapter(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Comments)
                .FirstOrDefaultAsync(c => c.ChapterID == id);

            if (chapter == null)
            {
                return NotFound();
            }
            //Không cho xem nếu truyện vẫn đang trạng thái bản thảo
            if (chapter.Status == ChapterStatus.Inactive)
            {
                return NotFound();
            }


            // Tên cookie riêng biệt cho chương truyện và người dùng
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string cookieName = currentUserId != null ? $"viewedchapter{id}_user_{currentUserId}" : $"viewedchapter{id}_guest";

            //Kiểm tra xem người dùng đã đăng nhập chưa
            if (!Request.Cookies.ContainsKey(cookieName))
            {
                //TODO: Chỉnh lại update view count theo cookie để chống spam
                chapter.ViewCount++;
                _context.Update(chapter);
                await _context.SaveChangesAsync();

                CookieOptions options = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30), // Thời gian sống của cookie
                                                                    // Có nghĩa là người sau 30p nữa mới tính là 1 lượt xem
                    HttpOnly = true,
                    IsEssential = true
                };

                Response.Cookies.Append(cookieName, "true", options);
            }

            var viewModel = new ChapterDetailViewModel
            {
                ChapterID = chapter.ChapterID,
                Title = chapter.Title,
                Content = chapter.Content,
                CreatedAt = chapter.CreatedAt,
                ViewCount = chapter.ViewCount,
                StoryTitle = chapter.Story?.Title ?? "Không rõ",
                StoryID = chapter.StoryID,
                Comments = chapter.Comments.OrderByDescending(c => c.CreatedAt).ToList()
            };

            return View(viewModel);
        }

        //POST: Chapter/CreateChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChapter(ChapterCreateViewModel chapter)
        {
            if (ModelState.IsValid)
            {
                var lastChapterOrder = _context.Chapters.Where(c => c.StoryID == chapter.StoryID)
                    .OrderByDescending(c => c.ChapterOrder)
                    .Select(c => c.ChapterOrder)
                    .FirstOrDefault();

                var newChapter = new ChapterModel
                {
                    Title = chapter.Title,
                    StoryID = chapter.StoryID,
                    Content = "",
                    Status = ChapterStatus.Inactive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ViewCount = 0,
                    ChapterOrder = lastChapterOrder + 1
                };

                await _context.Chapters.AddAsync(newChapter);
                await _context.SaveChangesAsync();

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


            TempData["Error"] = "Có lỗi xảy ra trong quá trình tạo chương mới. Vui lòng thử lại.";
            return RedirectToAction("EditDetail", "Story", new { id = chapter.StoryID });
        }

        //POST: Chapter/DeleteChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChapter(int chapterID, int storyID)
        {
            var chapter = await _context.Chapters.FindAsync(chapterID);
            if (chapter == null)
            {
                return NotFound();
            }

            var relatedComments = _context.Comments.Where(c => c.ChapterID == chapterID);
            _context.Comments.RemoveRange(relatedComments);

            // Cập nhật lại ChapterOrder cho các chương còn lại
            var chaptersToUpdate = await _context.Chapters
                .Where(c => c.StoryID == storyID && c.ChapterOrder > chapter.ChapterOrder)
                .ToListAsync();
            foreach (var c in chaptersToUpdate)
            {
                c.ChapterOrder--;
                _context.Chapters.Update(c);
            }

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            return RedirectToAction("EditDetail", "Story", new { id = storyID });
        }

        //GET: Chapter/EditChapter/{id}
        public async Task<IActionResult> EditChapter(int chapterId, int storyId)
        {
            var isAuthor = await _context.Stories
                .Where(s => s.StoryID == storyId)
                .Select(s => s.AuthorID)
                .FirstOrDefaultAsync();

            if (isAuthor == null)
            {
                return NotFound();
            }

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (isAuthor != currentUserId)
            {
                return RedirectToAction("AccessDenied", "Authentication");
            }

            var chapterContext = await _context.Chapters
                .Where(c => c.ChapterID == chapterId && c.StoryID == storyId)
                .FirstOrDefaultAsync();

            var viewModel = new ChapterEditViewModel
            {
                ChapterID = chapterId,
                StoryID = storyId,
                Title = chapterContext?.Title,
                Content = chapterContext?.Content,
                ChapterStatus = chapterContext?.Status ?? ChapterStatus.Inactive
            };


            return View(viewModel);
        }

        //POST: Chapter/EditChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChapter(ChapterEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình cập nhật chương. Vui lòng thử lại.";
                return View(model);
            }

            var chapter = await _context.Chapters.FindAsync(model.ChapterID);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = "Chương không tồn tại.";
                return View(model);
            }

            chapter.Title = model.Title;
            chapter.Content = model.Content;
            chapter.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật chương thành công.";
            return RedirectToAction("EditChapter", new
            {
                chapterId = model.ChapterID,
                storyId = model.StoryID
            });
        }
        // POST: Chapter/UpdateChapterStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateChapterStatus(int chapterId, string newStatus)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter == null)
            {
                TempData["ErrorMessage"] = "Chương không tồn tại.";
                return RedirectToAction("EditDetail", "Story", new { id = 0 }); 
            }

            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isAuthor = await _context.Stories
                                .Where(s => s.StoryID == chapter.StoryID)
                                .Select(s => s.AuthorID)
                                .FirstOrDefaultAsync();

            if (isAuthor != currentUserId)
            {
                return RedirectToAction("AccessDenied", "Authentication");
            }


            if (Enum.TryParse<ChapterStatus>(newStatus, out var parsedStatus))
            {
                chapter.Status = parsedStatus;
            }
            else
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                return RedirectToAction("EditChapter", new { chapterId = chapter.ChapterID, storyId = chapter.StoryID });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật trạng thái chương thành công.";
            return RedirectToAction("EditChapter", new { chapterId = chapter.ChapterID, storyId = chapter.StoryID });
        }

    }
}
