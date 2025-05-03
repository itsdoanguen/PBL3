using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.ViewModels.UserProfile;
using PBL3.Service;

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
        public IActionResult Index()
        {
            return View();
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
        public async Task<IActionResult> EditProfile(UserProfileViewModel profile, IFormFile? avatarUpload, IFormFile? bannerUpload) //Tham số IFormFile để lấy file ảnh từ form
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
            if(avatarUpload != null)
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
            UserProfileViewModel profile = new UserProfileViewModel();

            profile = await _userService.GetUserProfile(id);

            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        //GET: User/MyStories
        public async Task<IActionResult> MyStories()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var stories = await _userService.GetUserStoryCard(currentUserID);

            return View(stories);
        }
    }
}
