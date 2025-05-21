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

namespace PBL3.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private const string DefaultAvatar = "/image/default-avatar.png";
        private const string DefaultBanner = "/image/default-banner.png";

        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IUserService _userService;
        private readonly IImageService _imageService;
        
        public UserController(ApplicationDbContext context, BlobService blobService, IUserService userService, IImageService imageService)
        {
            _context = context;
            _blobService = blobService;
            _userService = userService;
            _imageService = imageService;
        }
        //GET: User/Index
        public async Task<IActionResult> Index()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var viewModel = await _userService.GetUserIndexViewModelAsync(currentUserID);

            return View(viewModel);
        }
        //GET: User/MyProfile
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

        //Đổi Mật Khẩu
        // GET: User/ChangePassword

        //

        //GET: User/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            UserProfileViewModel profile = await _userService.GetUserProfile(currentUserID);
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
            var userInfo = await _context.Users.FindAsync(currentUserID);
            if (userInfo == null)
            {
                return NotFound();
            }


            //Upload avatar
            if (avatarUpload != null)
            {
                var (isSuccess, errorMessage, imageUrl) = await _imageService.UploadValidateImageAsync(avatarUpload, "avatars");

                if (!isSuccess || string.IsNullOrEmpty(imageUrl))
                {
                    profile.Avatar = userInfo.Avatar;
                    profile.Banner = userInfo.Banner;
                    ModelState.AddModelError("Avatar", errorMessage);
                    return View(profile);
                }

                userInfo.Avatar = imageUrl;
            }
            else
            {
                userInfo.Avatar = profile.Avatar ?? DefaultAvatar;
            }

            //Upload banner
            if (bannerUpload != null)
            {
                var (isSuccess, errorMessage, imageUrl) = await _imageService.UploadValidateImageAsync(bannerUpload, "banners");
                if (!isSuccess || string.IsNullOrEmpty(imageUrl))
                {
                    profile.Avatar = userInfo.Avatar;
                    profile.Banner = userInfo.Banner;
                    ModelState.AddModelError("Banner", errorMessage);
                    return View(profile);
                }
                userInfo.Banner = imageUrl;
            }
            else
            {
                userInfo.Banner = profile.Banner ?? DefaultBanner;
            }

            userInfo.DisplayName = profile.DisplayName;
            userInfo.Bio = profile.Bio;
            userInfo.DateOfBirth = profile.DateOfBirth;
            userInfo.Gender = profile.Gender;

            _context.Users.Update(userInfo);
            await _context.SaveChangesAsync();

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

        // GET: User/Intro
        [AllowAnonymous]
        public async Task<IActionResult> Intro()
        {
            // Create a view model for the Intro page
            var viewModel = new IntroViewModel
            {
                HeaderMessage = "KHÁM PHÁ THẾ GIỚI TRUYỆN TIỂU THUYẾT CÙNG CHÚNG TÔI!",
                
                // Get hot stories from database
                HotStories = await GetHotStoriesAsync(),
                
                // Get new stories from database
                NewStories = await GetNewStoriesAsync(),
                
                // Get completed stories from database
                CompletedStories = await GetCompletedStoriesAsync(),
                
                // Get all categories for sidebar
                AllCategories = await GetAllCategoriesAsync(),
                
                // Create select list for category dropdown
                CategorySelectList = new SelectList(await GetAllCategoriesAsync(), "Id", "Name")
            };

            return View(viewModel);
        }

        // GET: User/GetHotStoriesByCategory
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetHotStoriesByCategory(int categoryId)
        {
            var stories = await GetHotStoriesByCategoryAsync(categoryId);
            return PartialView("_HotStoriesPartial", stories);
        }

        #region Helper Methods

        private async Task<List<StoryViewModel>> GetHotStoriesAsync()
        {
            return await _context.Stories
                .Where(s => s.Status == StoryModel.StoryStatus.Active)
                .OrderByDescending(s => s.CreatedAt)
                .Take(16)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = true, 
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryModel.StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count
                })
                .ToListAsync();
        }

        private async Task<List<StoryViewModel>> GetHotStoriesByCategoryAsync(int categoryId)
        {
            if (categoryId == 0)
            {
                return await GetHotStoriesAsync();
            }

            return await _context.Stories
                .Where(s => s.Status == StoryModel.StoryStatus.Active && 
                       s.Genres.Any(sg => sg.GenreID == categoryId))
                .OrderByDescending(s => s.CreatedAt)
                .Take(16)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = true, 
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryModel.StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count
                })
                .ToListAsync();
        }

        private async Task<List<StoryViewModel>> GetNewStoriesAsync()
        {
            return await _context.Stories
                .OrderByDescending(s => s.CreatedAt)
                .Take(15)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    IsHot = true, // For demo purposes
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryModel.StoryStatus.Completed,
                    Categories = s.Genres
                        .Select(sg => new CategoryViewModel
                        {
                            Id = sg.GenreID,
                            Name = sg.Genre.Name
                        })
                        .ToList(),
                    LatestChapter = s.Chapters
                        .OrderByDescending(c => c.ChapterOrder)
                        .Select(c => new ChapterViewModel
                        {
                            Id = c.ChapterID,
                            ChapterNumber = c.ChapterOrder,
                            Title = c.Title
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        private async Task<List<StoryViewModel>> GetCompletedStoriesAsync()
        {
            return await _context.Stories
                .Where(s => s.Status == StoryModel.StoryStatus.Completed)
                .OrderByDescending(s => s.CreatedAt)
                .Take(15)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    ChapterCount = s.Chapters.Count
                })
                .ToListAsync();
        }

        private async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            return await _context.Genres
                .OrderBy(g => g.Name)
                .Select(g => new CategoryViewModel
                {
                    Id = g.GenreID,
                    Name = g.Name
                })
                .ToListAsync();
        }

        #endregion
    }
}
