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


            //TODO: Chỉnh lại update view count theo cookie để chống spam
            chapter.ViewCount++;
            _context.Update(chapter);
            await _context.SaveChangesAsync();

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

            var relatedLikes = _context.LikeChapters.Where(l => l.ChapterID == chapterID);
            _context.LikeChapters.RemoveRange(relatedLikes);

            var relatedBookmarks = _context.Bookmarks.Where(b => b.ChapterID == chapterID);
            _context.Bookmarks.RemoveRange(relatedBookmarks);


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
    }
}
