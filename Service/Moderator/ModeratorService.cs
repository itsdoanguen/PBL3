using PBL3.Data;
using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;
using Microsoft.EntityFrameworkCore;
using PBL3.Service.Notification;
using PBL3.Service.Story;
using PBL3.Service.User;
using PBL3.Service.Report;

namespace PBL3.Service.Moderator
{
    public class ModeratorService : IModeratorService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly INotificationService _notificationService;
        private readonly IStoryQueryService _storyQueryService;
        private readonly IUserQueryService _userQueryService;
        private readonly IReportQueryService _reportQueryService;
        public ModeratorService(ApplicationDbContext context, BlobService blobService, INotificationService notificationService, IStoryQueryService storyQueryService, IUserQueryService userQueryService, IReportQueryService reportQueryService)
        {
            _blobService = blobService;
            _context = context;
            _notificationService = notificationService;
            _storyQueryService = storyQueryService;
            _userQueryService = userQueryService;
            _reportQueryService = reportQueryService;
        }
        public async Task<ViewUserViewModel> GetViewUserViewModelAsync(int userId)
        {
            return new ViewUserViewModel
            {
                userProfile = await _userQueryService.GetUserProfileAsync(userId),
                userStories = await _userQueryService.GetUserStoriesAsync(userId),
                UserComments = await _userQueryService.GetUserCommentsAsync(userId),
                ReportsCreated = await _reportQueryService.GetReportsCreatedByUserAsync(userId),
                ReportsReceived = await _reportQueryService.GetReportsReceivedByUserAsync(userId)
            };
        }
        public async Task<ViewStoryViewModel> GetViewStoryViewModelAsync(int storyId)
        {
            var storyVm = await _storyQueryService.GetStoryViewModelAsync(storyId);
            if (storyVm == null) return new ViewStoryViewModel();
            var story = await _context.Stories.Include(s => s.Author).FirstOrDefaultAsync(s => s.StoryID == storyId);
            if (story == null) return new ViewStoryViewModel();
            var author = await _userQueryService.GetUserProfileAsync(story.AuthorID) ?? new PBL3.ViewModels.Moderator.UserProfileViewModel();
            var chapters = await _storyQueryService.GetChaptersForStoryAsync(storyId) ?? new List<M_ChapterViewModel>();
            return new ViewStoryViewModel
            {
                Author = author,
                StoryDetails = storyVm,
                Chapters = chapters
            };
        }
        public async Task<List<ViewModels.Moderator.UserProfileViewModel>> GetListUserForModeratorAsync()
        {
            var users = await _context.Users.Where(u => u.Role == PBL3.Models.UserModel.UserRole.User)
                .Select(u => new ViewModels.Moderator.UserProfileViewModel
                {
                    UserID = u.UserID,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    Status = u.Status,
                    TotalWarning = u.TotalWarning
                })
                .ToListAsync();
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    user.Avatar = await _blobService.GetSafeImageUrlAsync(user.Avatar);
                }
            }
            return users;
        }
        public async Task<List<UserStoryCardViewModel>> GetListStoriesForModeratorAsync()
        {
            var stories = await _context.Stories.Include(s => s.Author)
                .Where(s => s.Status != PBL3.Models.StoryModel.StoryStatus.Inactive)
                .Select(s => new UserStoryCardViewModel
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    Cover = s.CoverImage,
                    TotalChapters = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    Status = s.Status
                })
                .ToListAsync();
            foreach (var story in stories)
            {
                if (!string.IsNullOrEmpty(story.Cover))
                    story.Cover = await _blobService.GetSafeImageUrlAsync(story.Cover);
            }
            return stories;
        }

        public async Task<(bool isSuccess, string errorMessage)> WarnUserAsync(int userId, string message, int moderatorId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "Không tìm thấy người dùng.");
            }

            user.TotalWarning += 1;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await _notificationService.InitNewWarningMessageAsync(userId, message, moderatorId);

            return (true, "Đã gửi cảnh báo đến User");
        }
        public async Task<(bool isSuccess, string errorMessage)> BanUserAsync(int userId, string message, int moderatorId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "Không tìm thấy người dùng.");
            }
            user.Status = Models.UserModel.UserStatus.Banned;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return (true, "Đã ban User thành công");
        }
        public async Task<(bool isSuccess, string errorMessage)> UnbanUserAsync(int userId, string message, int moderatorId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "Không tìm thấy người dùng.");
            }
            user.Status = Models.UserModel.UserStatus.Active;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            await _notificationService.InitNewWarningMessageAsync(userId, message, moderatorId);
            await _context.SaveChangesAsync();
            return (true, "Đã bỏ ban User thành công");
        }
    }
}
