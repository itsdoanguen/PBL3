using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Service.Discovery;
using PBL3.Service.Image;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.User
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IStoryRankingService _storyRankingService;
        private readonly IImageService _imageService;
        public UserService(ApplicationDbContext context, BlobService blobService, IStoryRankingService storyRankingService, IImageService imageService)
        {
            _context = context;
            _blobService = blobService;
            _storyRankingService = storyRankingService;
            _imageService = imageService;
        }
        public async Task<List<UserStoryCardViewModel>> GetUserStoryCard(int userId)
        {
            var stories = await _context.Stories
                .Where(s => s.AuthorID == userId)
                .Select(s => new UserStoryCardViewModel
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    Cover = s.CoverImage,
                    LastUpdated = s.UpdatedAt,
                    Status = s.Status,
                    TotalChapters = _context.Chapters.Count(c => c.StoryID == s.StoryID)
                })
                .ToListAsync();

            //Lấy URL của ảnh bìa từ blob
            foreach (var story in stories)
            {
                story.Cover = await _blobService.GetSafeImageUrlAsync(story.Cover ?? string.Empty);
            }
            return stories;
        }


        public async Task<UserProfileViewModel> GetUserProfile(int userId)
        {
            var userInfo = await _context.Users.FindAsync(userId);
            if (userInfo == null)
            {
                return null;
            }

            var stories = await GetUserStoryCard(userId);

            var avatarUrl = await _blobService.GetSafeImageUrlAsync(userInfo.Avatar ?? string.Empty);
            var bannerUrl = await _blobService.GetSafeImageUrlAsync(userInfo.Banner ?? string.Empty);

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
                TotalUploadedStories = stories.Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed).Count(),
                Stories = stories,
                TotalFollowers = _context.FollowUsers.Count(f => f.FollowingID == userId),
                TotalFollowings = _context.FollowUsers.Count(f => f.FollowerID == userId),
                TotalComments = _context.Comments.Count(c => c.UserID == userId)
            };

            return profile;

        }




        public async Task<(bool isSuccess, string errorMessage, UserProfileViewModel? updatedProfile)> UpdateUserProfileAsync(int userId, UserProfileViewModel profile, IFormFile? avatarUpload, IFormFile? bannerUpload)
        {
            var userInfo = await _context.Users.FindAsync(userId);
            if (userInfo == null)
            {
                return (false, "Không tìm thấy người dùng.", null);
            }

            // Upload avatar
            if (avatarUpload != null)
            {
                var (isSuccess, errorMessage, imageUrl) = await _imageService.UploadValidateImageAsync(avatarUpload, "avatars");
                if (!isSuccess || string.IsNullOrEmpty(imageUrl))
                {
                    var currentProfile = await GetUserProfile(userId);
                    return (false, errorMessage ?? string.Empty, currentProfile);
                }
                userInfo.Avatar = imageUrl;
            }

            // Upload banner
            if (bannerUpload != null)
            {
                var (isSuccess, errorMessage, imageUrl) = await _imageService.UploadValidateImageAsync(bannerUpload, "banners");
                if (!isSuccess || string.IsNullOrEmpty(imageUrl))
                {
                    var currentProfile = await GetUserProfile(userId);
                    return (false, errorMessage ?? string.Empty, currentProfile);
                }
                userInfo.Banner = imageUrl;
            }

            userInfo.DisplayName = profile.DisplayName;
            userInfo.Bio = profile.Bio;
            userInfo.DateOfBirth = profile.DateOfBirth;
            userInfo.Gender = profile.Gender;

            _context.Users.Update(userInfo);
            await _context.SaveChangesAsync();

            var updatedProfile = await GetUserProfile(userId);
            return (true, string.Empty, updatedProfile);
        }
        public async Task ToggleUpdateUserRoleAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Role = user.Role == Models.UserModel.UserRole.User ? Models.UserModel.UserRole.Moderator : Models.UserModel.UserRole.User;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<(bool isSuccess, string errorMessage)> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy người dùng.");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                return (false, "Mật khẩu cũ không đúng.");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return (false, "Mật khẩu mới phải có ít nhất 6 ký tự.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return (true, "Đổi mật khẩu thành công.");
        }
        public async Task<(bool isSuccess, string errorMessage)> ForgotPasswordAsync(string email, Func<string, string, string?, Task> sendEmailFunc)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return (false, "Vui lòng nhập email đã đăng ký.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return (false, "Email không tồn tại trong hệ thống.");
            }
            // Random mật khẩu mới
            var newPassword = GenerateRandomPassword(8);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            await sendEmailFunc(user.Email, newPassword, user.DisplayName);
            return (true, "Mật khẩu mới đã được gửi về email của bạn. Vui lòng đăng nhập lại.");
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
