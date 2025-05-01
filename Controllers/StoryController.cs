using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels;
using PBL3.ViewModels.Chapter;
using PBL3.ViewModels.Story;
using PBL3.Service;

namespace PBL3.Controllers
{
    public class StoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IChapterService _chapterService;
        public StoryController(ApplicationDbContext context, BlobService blobService, IChapterService chapterService)
        {
            _context = context;
            _blobService = blobService;
            _chapterService = chapterService;
        }

        //GET: Story/ViewStory
        //TODO: Lấy các thông tin của truyện, cũng như hiển thị các comment,....


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
            if (model.GenreIDs == null || !model.GenreIDs.Any())
            {
                ModelState.AddModelError("GenreIDs", "Thể loại là bắt buộc, hãy chọn ít nhất 1");
                model.availbleGenres = GetAvailbleGenres();
                return View(model);
            }


            var existingStory = _context.Stories.FirstOrDefault(s => s.Title == model.Title);
            if (existingStory != null)
            {
                ModelState.AddModelError("Title", "Tên truyện đã tồn tại");
                model.availbleGenres = GetAvailbleGenres();
                return View(model);
            }

            string coverImagePath = "/image/default-cover.jpg";

            if (model.UploadCover != null && model.UploadCover.Length > 0)
            {
                if (model.UploadCover.Length > 1024 * 1024)
                {
                    ModelState.AddModelError("UploadCover", "Size ảnh phải nhỏ hơn 1MB");
                    model.availbleGenres = GetAvailbleGenres();
                    return View(model);
                }

                var fileExtension = Path.GetExtension(model.UploadCover.FileName);
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    ModelState.AddModelError("UploadCover", "Ảnh phải thuộc định dạng jpg, jpeg hoặc png");
                    model.availbleGenres = GetAvailbleGenres();
                    return View(model);
                }

                var fileName = $"covers/{Guid.NewGuid()}_cover{fileExtension}";
                using (var stream = model.UploadCover.OpenReadStream())
                {
                    var result = await _blobService.UploadFileAsync(stream, fileName);
                    coverImagePath = result;
                }
            }

            var newStory = new StoryModel
            {
                Title = model.Title,
                Description = model.Description,
                CoverImage = coverImagePath,
                Status = StoryModel.StoryStatus.Inactive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                AuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Genres = new List<StoryGenreModel>()
            };

            await _context.Stories.AddAsync(newStory);
            await _context.SaveChangesAsync();

            if (model.GenreIDs != null && model.GenreIDs.Count > 0)
            {
                foreach (var genre in model.GenreIDs)
                {
                    _context.StoryGenres.Add(new StoryGenreModel
                    {
                        StoryID = newStory.StoryID,
                        GenreID = genre
                    });
                }
                await _context.SaveChangesAsync();
            }

            
            return RedirectToAction("MyStories", "User");
        }

        ////POST: Story/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int StoryID)
        {
            // Xóa cmt của story
            var relatedCommentsStory = await _context.Comments
                .Where(c => c.StoryID == StoryID)
                .ToListAsync();
            foreach (var comment in relatedCommentsStory)
            {
                _context.Comments.Remove(comment);
            }


            // Xóa chapter từ IChapterSevice
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == StoryID)
                .ToListAsync();
            foreach (var chapter in chapters)
            {
                await _chapterService.DeleteChapterAsync(chapter.ChapterID, StoryID);
            }

            // Xóa Story
            var story = await _context.Stories
                .Where(s => s.StoryID == StoryID)
                .FirstOrDefaultAsync();
            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyStories", "User");
        }
        


        //GET: Story/EditDetail/{id}
        public async Task<IActionResult> EditDetail(int id)
        {
            int currentAuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var story = await _context.Stories
                .Where(s => s.AuthorID == currentAuthorID && s.StoryID == id)
                .FirstOrDefaultAsync();

            if (story == null)
            {
                return NotFound();
            }

            var chapterList = await GetChaptersForStoryAsync(id);

            var totalLike = await _context.LikeChapters
                .Where(l => l.Chapter.StoryID == id)
                .CountAsync();

            var totalBookmark = await _context.Bookmarks
                .Where(b => b.Chapter.StoryID == id)
                .CountAsync();

            var totalComment = await _context.Comments
                .Where(c => c.Chapter.StoryID == id)
                .CountAsync();

            var totalView = await _context.Chapters
                .Where(c => c.StoryID == id)
                .SumAsync(c => c.ViewCount);

            var viewModel = new StoryEditViewModel
            {
                StoryID = story.StoryID,
                Title = story.Title,
                Description = story.Description,
                CoverImage = story.CoverImage,
                TotalLike = totalLike,
                TotalBookmark = totalBookmark,
                TotalComment = totalComment,
                TotalChapter = chapterList.Count,
                TotalView = totalView,
                Chapters = chapterList
            };

            return View(viewModel);
        }



        //METHOD
        private async Task<List<ChapterSummaryViewModel>> GetChaptersForStoryAsync(int storyId)
        {
            return await _context.Chapters
                .Where(c => c.StoryID == storyId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ChapterSummaryViewModel
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ViewCount = c.ViewCount,
                    ChapterOrder = c.ChapterOrder,
                    Status = (ChapterSummaryViewModel.ChapterStatus)c.Status
                })
                .ToListAsync();
        }

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
