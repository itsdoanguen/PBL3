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
        // GET: Chapter/CreateChapterPartial
        public IActionResult CreateChapterPartial(int storyId)
        {
            var model = new ChapterCreateViewModel
            {
                StoryID = storyId
            };
            return PartialView("_CreateChapterPartial", model);
        }

        // POST: Chapter/CreateChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChapter(ChapterCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var chapter = new ChapterModel
                {
                    StoryID = model.StoryID,
                    Title = model.Title,
                    Content = model.Content,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ViewCount = 0
                };

                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return PartialView("_CreateChapterPartial", model);
        }
    }
}
