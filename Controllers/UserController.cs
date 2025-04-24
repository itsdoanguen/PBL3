using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.ViewModels.UserProfile;

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
        public async Task<IActionResult> MyProfile()
        {
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            //Dùng GetUserProfile để lấy thông tin của user hiện tại, tương tự cho các hàm ở dưới
            UserProfileViewModel profile = await GetUserProfile(currentUserID);

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
            UserProfileViewModel profile = await GetUserProfile(currentUserID);
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
            //TODO: remove đoạn code check đuôi file, allow all file type nhưng vấn đề là nếu ko phải file ảnh thì... nghiên cứu thêm

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

            profile = await GetUserProfile(id);

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
            var stories = await GetUserStoryCard(currentUserID);

            return View(stories);
        }

        //Hàm private lấy thông tin của user, trả về kiểu UserProfileViewModel dùng để hiển thị thông tin của user ở một số trang như MyProfile, ViewProfile, EditProfile
        private async Task<UserProfileViewModel> GetUserProfile(int id)
        {
            var userInfo = await _context.Users.FindAsync(id);
            if (userInfo == null)
            {
                return null;
            }

            var stories = await GetUserStoryCard(id);


            //Lấy URL của avatar và banner, sau đó tách tên blob
            string avatarBlobName = ExtractBlobName(userInfo.Avatar);
            string bannerBlobName = ExtractBlobName(userInfo.Banner);

            //Kiểm tra nếu có avatar hoặc banner thì lấy URL của blob, còn không thì trả về null
            var avatarUrl = string.IsNullOrEmpty(avatarBlobName) ? null : await _blobService.GetBlobSasUrlAsync(avatarBlobName);
            var bannerUrl = string.IsNullOrEmpty(bannerBlobName) ? null : await _blobService.GetBlobSasUrlAsync(bannerBlobName);

            var profile = new UserProfileViewModel
            {
                DisplayName = userInfo.DisplayName,
                Email = userInfo.Email,
                Avatar = avatarUrl,
                Banner = bannerUrl,
                Bio = userInfo.Bio,
                CreatedAt = userInfo.CreatedAt,
                DateOfBirth = userInfo.DateOfBirth,
                Role = userInfo.Role,
                Gender = userInfo.Gender,
                Status = userInfo.Status,
                TotalUploadedStories = stories.Where(s => s.Status == Models.StoryModel.StoryStatus.Active).Count(),
                Stories = stories,
                TotalFollowers = _context.FollowUsers.Count(f => f.FollowingID == id),
                TotalFollowings = _context.FollowUsers.Count(f => f.FollowerID == id),
                TotalComments = _context.Comments.Count(c => c.UserID == id)
            };

            return profile;
        }

        private async Task<List<UserStoryCardViewModel>> GetUserStoryCard(int id)
        {
            var stories = await _context.Stories
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
                .ToListAsync();

            return stories;
        }


        //Hàm tách tên blob từ URL
        private string ExtractBlobName(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            //Vì URL có định dạng là "https://<storageName>.blob.core.windows.net/<containerName>/<blobName>" 
            //Dùng hàm IndexOf để tìm vị trí của tên container
            //Sau đó lấy phần còn lại của URL từ vị trí của container đến hết
            string containerName = "pbl3container/";
            int index = url.IndexOf(containerName);

            if (index != -1)
            {
                return url.Substring(index + containerName.Length);
            }

            return url;
        }
    }
}
