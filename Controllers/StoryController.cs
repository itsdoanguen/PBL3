using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.Service.Chapter;
using PBL3.Service.Discovery;
using PBL3.Service.Story;
using PBL3.ViewModels.Story;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Controllers
{
    [Authorize]
    public class StoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IChapterService _chapterService;
        private readonly IStoryService _storyService;
        private readonly IStoryQueryService _storyQueryService;
        private readonly IStoryRankingService _storyRankingService;
        public StoryController(ApplicationDbContext context, BlobService blobService, IChapterService chapterService, IStoryService storyService, IStoryQueryService storyQueryService, IStoryRankingService storyRankingService)
        {
            _context = context;
            _blobService = blobService;
            _chapterService = chapterService;
            _storyService = storyService;
            _storyQueryService = storyQueryService;
            _storyRankingService = storyRankingService;
        }

        // GET: Story/View/{id}
        public async Task<IActionResult> View(int id)
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var storyDetail = await _storyQueryService.GetStoryDetailAsync(id, currentUserID);
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
                // Cập nhật lại trạng thái IsSelected dựa vào GenreIDs
                var allGenres = GetAvailbleGenres();
                foreach (var genre in allGenres)
                {
                    genre.IsSelected = model.GenreIDs != null && model.GenreIDs.Contains(genre.GenreID);
                }
                model.availbleGenres = allGenres;
                return View(model);
            }

            int authorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var (isSuccess, errorMessage, storyID) = await _storyService.CreateStoryAsync(model, authorID);

            if (!isSuccess)
            {
                // Cập nhật lại trạng thái IsSelected dựa vào GenreIDs
                var allGenres = GetAvailbleGenres();
                foreach (var genre in allGenres)
                {
                    genre.IsSelected = model.GenreIDs != null && model.GenreIDs.Contains(genre.GenreID);
                }
                model.availbleGenres = allGenres;
                TempData["ErrorMessage"] = errorMessage;
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

            var viewModel = await _storyQueryService.GetStoryDetailForEditAsync(id, currentAuthorID);

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
            var viewModel = await _storyQueryService.GetStoryDetailForEditAsync(id, currentAuthorID);
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
                // Reload AvailableGenres if validation fails
                model.AvailableGenres = _context.Genres.Select(g => new GerneVM
                {
                    GenreID = g.GenreID,
                    Name = g.Name
                }).ToList();
                return View(model);
            }
            
            int currentAuthorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _storyService.UpdateStoryAsync(model, currentAuthorID);
            
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                // Reload AvailableGenres if service fails
                model.AvailableGenres = _context.Genres.Select(g => new GerneVM
                {
                    GenreID = g.GenreID,
                    Name = g.Name
                }).ToList();
                return View(model);
            }
            
            TempData["SuccessMessage"] = "Cập nhật truyện thành công!";
            return RedirectToAction("EditDetail", new { id = model.StoryID });
        }
        //POST: Story/PendingReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PendingReview(int storyId)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("EditDetail", new { id = storyId });
            }
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _storyService.PendingReviewAsync(storyId, currentUserID);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("EditDetail", new { id = storyId });
            }
            TempData["SuccessMessage"] = "Đã gửi truyện chờ duyệt thành công!";
            return RedirectToAction("MyStories", "User");
        }

        //POST: Story/Lock
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Lock(int storyID, string message)
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _storyService.LockStoryAsync(storyID, message, currentUserID);

            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("ViewStory", "Moderator", new { id = storyID });
            }
            TempData["SuccessMessage"] = "Khóa truyện thành công!";
            return RedirectToAction("ViewStory", "Moderator", new { id = storyID });
        }
        //POST: Story/Unlock
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Unlock(int storyID, string message, bool isAccepted)
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _storyService.UnlockStoryAsync(storyID, isAccepted, message, currentUserID);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("ViewStory", "Moderator", new { id = storyID });
            }
            TempData["SuccessMessage"] = "Mở khóa truyện thành công!";
            return RedirectToAction("ViewStory", "Moderator", new { id = storyID });
        }
        //POST: Story/AddGenre
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddGenre(string genreName)
        {
            var (isSuccess, errorMessage) = await _storyService.AddNewGenreAsync(genreName);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("ManageSystem", "Admin");
            }
            TempData["SuccessMessage"] = "Thêm thể loại mới thành công!";
            return RedirectToAction("ManageSystem", "Admin");
        }
        //POST: Story/DeleteGenre
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int genreId)
        {
            var (isSuccess, errorMessage) = await _storyService.DeleteGenreAsync(genreId);
            if (!isSuccess)
            {
                TempData["ErrorMessage"] = errorMessage;
                return RedirectToAction("ManageSystem", "Admin");
            }
            TempData["SuccessMessage"] = "Xóa thể loại thành công!";
            return RedirectToAction("ManageSystem", "Admin");
        }

        //GET: Story/AllStories?query
        public async Task<IActionResult> AllStories(string? query = null, int page = 1, int pageSize = 10)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int currentUserID = 0;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                currentUserID = int.Parse(userIdClaim);
            }

            List<UserStoryCardViewModel> stories = new List<UserStoryCardViewModel>();
            if (string.IsNullOrEmpty(query))
            {
                query = "updated";
            }
            switch (query.ToLower())
            {
                case "view":
                    // Lấy truyện theo lượt xem
                    stories = await _storyRankingService.GetStoriesByViewAsync();
                    break;
                case "follow":
                    // Lấy truyện theo lượt follow
                    stories = await _storyRankingService.GetStoriesByFollowAsync();
                    break;
                case "recommend":
                    // Lấy truyện đề xuất cho user
                    stories = await _storyRankingService.GetRecommendedStoryAsync(currentUserID);
                    break;
                case "wordcount":
                    // Lấy truyện theo số lượng từ
                    stories = await _storyRankingService.GetStoriesByWordCountAsync();
                    break;
                case "like":
                    // Lấy truyện theo lượt thích 
                    stories = await _storyRankingService.GetStoriesByLikeAsync();
                    break;
                case "updated":
                    // Lấy truyện mới cập nhật
                    stories = await _storyRankingService.GetStoriesByUpdatedAsync();
                    break;
                default:
                    stories = await _storyRankingService.GetStoriesByUpdatedAsync();
                    break;
            }

            ViewBag.CurrentQuery = query;
            return View(stories);
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
