using Microsoft.AspNetCore.Mvc;
using PBL3.Models;
using PBL3.Data;
using System.Security.Claims;
using PBL3.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace PBL3.Controllers
{
    public class StoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public StoryController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        //GET: Story/ViewStory
        //TODO: Lấy các thông tin của truyện, cũng như hiển thị các comment,....


        // GET: Story/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Story/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var existingStory = _context.Stories.FirstOrDefault(s => s.Title == model.Title);
            if (existingStory != null)
            {
                ModelState.AddModelError("Title", "Title already exists");
                return View(model);
            }

            string coverImagePath = "/image/default-cover.jpg"; 

            if (model.UploadCover != null && model.UploadCover.Length > 0)
            {
                if (model.UploadCover.Length > 1024 * 1024)
                {
                    ModelState.AddModelError("UploadCover", "Cover image must be less than 1MB");
                    return View(model);
                }

                var fileExtension = Path.GetExtension(model.UploadCover.FileName);
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    ModelState.AddModelError("UploadCover", "Cover image must be in jpg, jpeg or png format");
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
                AuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))
            };

            await _context.Stories.AddAsync(newStory);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyStories", "User");
        }

        //GET: Story/Detail/{id}
        //TODO: Hiển thị các thông tin truyện cho tác giả có thể edit
        public async Task<IActionResult> Detail(int id)
        {
            int currentAuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var stories = await _context.Stories.Where(s => s.AuthorID == currentAuthorID && s.StoryID == id).FirstOrDefaultAsync();

            if (stories == null)
            {
                return NotFound();
            }

            var detail = new StoryEditViewModel
            {
                Title = stories.Title,
                Description = stories.Description,
                CoverImage = stories.CoverImage,
                Chapters = await _context.Chapters.Where(s => s.StoryID == id).ToListAsync()
            };
            return View(detail);
        }

    }
}
