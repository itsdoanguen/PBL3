using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using PBL3.ViewModels.User;
using PBL3.Models;
using PBL3.Data;
using PBL3.Service.Image;
using PBL3.Service.User;
using PBL3.ViewModels.UserProfile;
using PBL3.Service;
using Microsoft.EntityFrameworkCore;
using PBL3.Service.Bookmark;
using PBL3.Service.Follow;
using PBL3.Service.Dashboard;   


namespace PBL3.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private const string DefaultAvatar = "/image/default-avatar.png";
        private const string DefaultBanner = "/image/default-banner.png";

        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly BlobService _blobService;
        private readonly IImageService _imageService;
        private readonly IBookmarkService _bookmarkService;
        private readonly IFollowService _followService;
        private readonly IDashboardService _dashboardService;
        

        public UserController(ApplicationDbContext context, BlobService blobService, IUserService userService, IImageService imageService, IBookmarkService bookmarkService, IFollowService followService, IDashboardService dashboardService)
        {
            _context = context;
            _blobService = blobService;
            _userService = userService;
            _imageService = imageService;
            _bookmarkService = bookmarkService;
            _followService = followService;
            _dashboardService = dashboardService;
        }
        //GET: User/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? currentUserID = null;
            if (User.Identity.IsAuthenticated)
            {
                currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var viewModel = await _dashboardService.GetDashboardDataAsync(currentUserID);

            return View(viewModel);
        }
        //GET: User/MyProfile
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //Dùng GetUserProfile để lấy thông tin của user hiện tại, tương tự cho các hàm ở dưới
            UserProfileViewModel profile = await _userService.GetUserProfile(currentUserID);

            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        //GET: User/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var profile = await _userService.GetUserProfile(currentUserID);
            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }
        //POST: User/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UserProfileViewModel profile, IFormFile? avatarUpload, IFormFile? bannerUpload)
        {
            if (!ModelState.IsValid)
            {
                return View(profile);
            }

            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage, updatedProfile) = await _userService.UpdateUserProfileAsync(currentUserID, profile, avatarUpload, bannerUpload);
            if (!isSuccess)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
                return View(updatedProfile ?? profile);
            }
            return RedirectToAction("MyProfile", "User");
        }

        //GET: User/ViewProfile
        public async Task<IActionResult> ViewProfile(int id)
        {
            int currentUserID = 0;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
                currentUserID = int.Parse(userIdClaim.Value);

            var followService = HttpContext.RequestServices.GetService(typeof(PBL3.Service.Follow.IFollowService)) as PBL3.Service.Follow.IFollowService;
            bool isFollowed = false;
            if (currentUserID != 0 && currentUserID != id && followService != null)
            {
                isFollowed = await followService.IsFollowingUserAsync(currentUserID, id);
            }

            var profile = await _userService.GetUserProfile(id);
            if (profile == null)
            {
                return NotFound();
            }
            profile.IsFollowed = isFollowed;
            return View(profile);
        }

        //GET: User/MyStories
        public async Task<IActionResult> MyStories()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var stories = await _userService.GetUserStoryCard(currentUserID);

            return View(stories);
        }

        //GET: User/Library
        public async Task<IActionResult> Library()
        {
            return View();
        }

        //GET: User/Bookmarks
        public async Task<IActionResult> Bookmarks()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var bookmarks = await _bookmarkService.GetBookmarkListAsync(currentUserID);
            return View(bookmarks);
        }

        //GET: User/FollowStories
        public async Task<IActionResult> FollowStories()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var followService = await _followService.GetFollowStoryList(currentUserID);
            return View(followService);
        }

        //POST: User/ToggleUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserRole(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "ID không hợp lệ.";
                return RedirectToAction("ManageSystem", "Admin");
            }

            try
            {
                await _userService.ToggleUpdateUserRoleAsync(id);
                TempData["SuccessMessage"] = "Cập nhật vai trò người dùng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error khi update user role: {ex.Message}";
            }

            return RedirectToAction("ManageSystem", "Admin");
        }

        //GET: User/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //POST: User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận không khớp.");
                return View();
            }
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _userService.ChangePasswordAsync(currentUserID, oldPassword, newPassword);
            if (!isSuccess)
            {
                ModelState.AddModelError("", errorMessage);
                return View();
            }
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("MyProfile");
        }

        //GET: User/LibraryFollow
        public async Task<IActionResult> LibraryFollow()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var following = await _followService.GetFollowingUsersAsync(currentUserID);
            var followers = await _followService.GetFollowerUsersAsync(currentUserID);
            var model = new PBL3.ViewModels.FollowUser.UserFollowListViewModel
            {
                FollowingUsers = following,
                FollowerUsers = followers
            };
            return View(model);
        }

        //GET: User/GetHotStoriesByCategory
        [HttpGet]
        public async Task<IActionResult> GetHotStoriesByCategory(int categoryId)
        {
            var stories = await _dashboardService.GetHotStoriesByCategoryAsync(categoryId, 20);
            return PartialView("_HotStoriesPartial", stories);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestData()
        {
            try
            {
                // Test database connection
                var storyCount = await _context.Stories.CountAsync();
                var userCount = await _context.Users.CountAsync();
                var genreCount = await _context.Genres.CountAsync();
                var chapterCount = await _context.Chapters.CountAsync();

                var testInfo = new
                {
                    DatabaseConnected = true,
                    StoriesCount = storyCount,
                    UsersCount = userCount,
                    GenresCount = genreCount,
                    ChaptersCount = chapterCount,
                    Message = "Database connection successful"
                };

                return Json(testInfo);
            }
            catch (Exception ex)
            {
                var errorInfo = new
                {
                    DatabaseConnected = false,
                    Error = ex.Message,
                    Message = "Database connection failed"
                };

                return Json(errorInfo);
            }
        }
    }
}
