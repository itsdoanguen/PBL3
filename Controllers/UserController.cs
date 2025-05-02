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
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IUserService _userService;
        public UserController(ApplicationDbContext context, BlobService blobService, IUserService userService)
        {
            _context = context;
            _blobService = blobService;
            _userService = userService;
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
            var userInfo = _context.Users.Find(currentUserID);
            if (userInfo == null)
            {
                return NotFound();
            }

            //Xử lý upload avatar và banner, ở đây giới hạn kích thước file là 1MB và chỉ cho phép file ảnh có đuôi là jpg, jpeg, png
            if (avatarUpload != null && avatarUpload.Length > 0)
            {
                if (avatarUpload.Length > 1024 * 1024)
                {
                    ModelState.AddModelError("Avatar", "Avatar must be less than 1MB");
                    return View(profile);
                }
                var fileExtension = Path.GetExtension(avatarUpload.FileName); //Lấy đuôi file
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    ModelState.AddModelError("Avatar", "Avatar must be in jpg, jpeg or png format");
                    return View(profile);
                }

                var fileName = $"avatars/{currentUserID}_avatar{fileExtension}"; //Tạo blob name ở chỗ này
                using (var stream = avatarUpload.OpenReadStream())
                {
                    //Dùng service đã định gnhiax ở BlobService để upload file 
                    var result = await _blobService.UploadFileAsync(stream, fileName);
                    userInfo.Avatar = result;
                }
            }
            else
            {
                //Nếu không user không upload avatar trong form edit thì lấy avatar cũ
                userInfo.Avatar = profile.Avatar ?? "/image/default-avatar.png";
            }
            //Thằng này tương tự với avatar
            if (bannerUpload != null && bannerUpload.Length > 0)
            {
                if (bannerUpload.Length > 1024 * 1024)
                {
                    ModelState.AddModelError("Banner", "Banner must be less than 1MB");
                    return View(profile);
                }
                var fileExtension = Path.GetExtension(bannerUpload.FileName);
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    ModelState.AddModelError("Banner", "Banner must be in jpg, jpeg or png format");
                    return View(profile);
                }
                var fileName = $"banners/{currentUserID}_banner{fileExtension}";
                using (var stream = bannerUpload.OpenReadStream())
                {
                    var result = await _blobService.UploadFileAsync(stream, fileName);
                    userInfo.Banner = result;
                }
            }
            else
            {
                userInfo.Banner = profile.Banner;
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
