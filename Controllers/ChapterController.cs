using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels;
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
                var newChapter = new ChapterModel
                {
                    Title = chapter.Title,
                    StoryID = chapter.StoryID,
                    Content = "",
                    Status = ChapterStatus.Inactive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ViewCount = 0
                };

                await _context.Chapters.AddAsync(newChapter);
                await _context.SaveChangesAsync();

                return RedirectToAction("EditDetail", "Story", new {id = chapter.StoryID});
            }


            TempData["Error"] = "Có lỗi xảy ra trong quá trình tạo chương mới. Vui lòng thử lại.";
            return RedirectToAction("EditDetail", "Story", new { id = chapter.StoryID });
        }

    }
}
