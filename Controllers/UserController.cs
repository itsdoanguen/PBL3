using System.Security.Claims;
using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.ViewModels;

namespace PBL3.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public UserController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }
        //GET: User/Index
        public IActionResult Index()
        {
            return View();
        }
        //GET: User/MyProfile
        public IActionResult MyProfile()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            UserProfileViewModel profile = GetUserProfile(currentUserID);

            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        //GET: User/EditProfile
        public IActionResult EditProfile()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            UserProfileViewModel profile = GetUserProfile(currentUserID);
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
            var userInfo = _context.Users.Find(currentUserID);
            if (userInfo == null)
            {
                return NotFound();
            }
            
            if (avatarUpload != null && avatarUpload.Length > 0)
            {
                if (avatarUpload.Length > 1024 * 1024)
                {
                    ModelState.AddModelError("Avatar", "Avatar must be less than 1MB");
                    return View(profile);
                }
                var fileExtension = Path.GetExtension(avatarUpload.FileName);
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    ModelState.AddModelError("Avatar", "Avatar must be in jpg, jpeg or png format");
                    return View(profile);
                }
                var fileName = $"avatars/{currentUserID}_avatar{fileExtension}";
                using (var stream = avatarUpload.OpenReadStream())
                {
                    var result = await _blobService.UploadFileAsync(stream,fileName);
                    userInfo.Avatar = result;
                }
            } else
            {
                userInfo.Avatar = profile.Avatar ?? "/image/default-avatar.png";
            }

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
            } else
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
        public IActionResult ViewProfile(int id)
        {
            UserProfileViewModel profile = new UserProfileViewModel();

            profile = GetUserProfile(id);

            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        private UserProfileViewModel GetUserProfile(int id)
        {
            var userInfo = _context.Users.Find(id);
            if (userInfo == null)
            {
                return null;
            }

            var stories = _context.Stories
                .Where(s => s.AuthorID == id)
                .Select(s => new UserStoryCardViewModel
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    Cover = s.CoverImage,
                    LastUpdated = s.UpdatedAt,
                    Status = s.Status,
                    TotalChapters = _context.Chapters.Count(c => c.StoryID == s.StoryID),
                })
                .ToList();

            var profile = new UserProfileViewModel
            {
                DisplayName = userInfo.DisplayName,
                Email = userInfo.Email,
                Avatar = userInfo.Avatar,
                Banner = userInfo.Banner,
                Bio = userInfo.Bio,
                CreatedAt = userInfo.CreatedAt,
                DateOfBirth = userInfo.DateOfBirth,
                Role = userInfo.Role,
                Gender = userInfo.Gender,
                Status = userInfo.Status,
                TotalUploadedStories = stories.Count,
                Stories = stories,
                TotalFollowers = _context.FollowUsers.Count(f => f.FollowingID == id),
                TotalFollowings = _context.FollowUsers.Count(f => f.FollowerID == id),
                TotalComments = _context.Comments.Count(c => c.UserID == id)
            };

            return profile;
        }
    }
}
