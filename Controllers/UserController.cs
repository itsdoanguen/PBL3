using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Bookmark;
using PBL3.Service.Dashboard;
using PBL3.Service.Email;
using PBL3.Service.Follow;
using PBL3.Service.User;
using PBL3.ViewModels.Account;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IBookmarkService _bookmarkService;
        private readonly IFollowService _followService;
        private readonly IDashboardService _dashboardService;
        private readonly IEmailService _emailService;

        public UserController(IUserService userService, IBookmarkService bookmarkService, IFollowService followService, IEmailService emailService, IDashboardService dashboardService)
        {
            _userService = userService;
            _bookmarkService = bookmarkService;
            _followService = followService;
            _dashboardService = dashboardService;
            _emailService = emailService;
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
            var ChangePasswordViewModel = new ChangePasswordViewModel
            {
                OldPassword = string.Empty,
                NewPassword = string.Empty,
                ConfirmPassword = string.Empty
            };
            return View(ChangePasswordViewModel);
        }

        //POST: User/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OldPassword) || string.IsNullOrWhiteSpace(model.NewPassword) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                return View(model);
            }
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận không khớp.");
                return View(model);
            }
            int currentUserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (isSuccess, errorMessage) = await _userService.ChangePasswordAsync(currentUserID, model.OldPassword, model.NewPassword);
            if (!isSuccess)
            {
                ModelState.AddModelError("", errorMessage);
                return View(model);
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
            var model = new ViewModels.FollowUser.UserFollowListViewModel
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

        //GET: User/Settings
        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }
        //GET: User/ForgotPassword
        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }
        //POST: User/ForgotPassword
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var (isSuccess, message) = await _userService.ForgotPasswordAsync(
                model.Email,
                async (toEmail, newPassword, displayName) =>
                {
                    await SendResetPasswordEmail(toEmail, newPassword, displayName);
                });
            if (!isSuccess)
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError("Email", message);
                else
                    TempData["ErrorMessage"] = message;
                return View(model);
            }
            // Nếu người dùng đang đăng nhập, đăng xuất họ ra
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            TempData["SuccessMessage"] = message;
            return RedirectToAction("Login", "Authentication");
        }

        // Hàm gửi email
        private async Task SendResetPasswordEmail(string toEmail, string newPassword, string? displayName = null)
        {
            var subject = "Mật khẩu mới cho tài khoản PBL3";
            var body = _emailService.GetForgotPasswordEmailBody(displayName, newPassword);
            await _emailService.SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
    }
}
