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

        public UserController(ApplicationDbContext context, BlobService blobService, IUserService userService, IImageService imageService, IBookmarkService bookmarkService, IFollowService followService)
        {
            _context = context;
            _blobService = blobService;
            _userService = userService;
            _imageService = imageService;
            _bookmarkService = bookmarkService;
            _followService = followService;
        }
        //GET: User/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var viewModel = await _userService.GetUserIndexViewModelAsync(currentUserID);

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
    }
}
