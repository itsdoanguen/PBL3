using PBL3.Data;
using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;
using Microsoft.EntityFrameworkCore;
using PBL3.Service.Notification;

namespace PBL3.Service.Moderator
{
    public class ModeratorService : IModeratorService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly INotificationService _notificationService;
        public ModeratorService(ApplicationDbContext context, BlobService blobService, INotificationService notificationService)
        {
            _blobService = blobService;
            _context = context;
            _notificationService = notificationService;
        }
        public async Task<ViewUserViewModel> GetViewUserViewModelAsync(int userId)
        {
            return new ViewUserViewModel
            {
                userProfile = await GetUserProfileAsync(userId),
                userStories = await GetUserStoriesAsync(userId),
                UserComments = await GetUserCommentsAsync(userId),
                ReportsCreated = await GetReportsCreatedByUserAsync(userId),
                ReportsReceived = await GetReportsReceivedByUserAsync(userId)
            };
        }
        public async Task<ViewStoryViewModel> GetViewStoryViewModelAsync(int storyId)
        {
            var story = await _context.Stories.Include(s => s.Author)
                .FirstOrDefaultAsync(s => s.StoryID == storyId);
            if (story == null) return null;

            var author = await GetUserProfileAsync(story.AuthorID);
            var chapters = await GetChaptersForStoryAsync(storyId);

            var storyVm = new M_StoryViewModel
            {
                StoryID = story.StoryID,
                Title = story.Title,
                CoverImage = story.CoverImage,
                Description = story.Description,
                Status = story.Status,
                CreatedAt = story.CreatedAt,
                UpdatedAt = story.UpdatedAt,
                isReported = await IsStoryReportedAsync(storyId)
            };
            if (!string.IsNullOrEmpty(storyVm.CoverImage))
            {
                storyVm.CoverImage = await _blobService.GetSafeImageUrlAsync(storyVm.CoverImage);
            }
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
                    Status = u.Status
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

        // --- PRIVATE HELPERS ---
        private async Task<List<M_ChapterViewModel>> GetChaptersForStoryAsync(int storyId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyId)
                .OrderBy(c => c.ChapterOrder)
                .Select(c => new M_ChapterViewModel
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    ChapterOrder = c.ChapterOrder,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Status = c.Status,
                    ViewCount = c.ViewCount,
                    isReported = _context.Notifications.Any(n => n.ChapterID == c.ChapterID && (n.Type == PBL3.Models.NotificationModel.NotificationType.ReportChapter) && n.IsRead == false)
                })
                .ToListAsync();
            return chapters;
        }

        private async Task<bool> IsStoryReportedAsync(int storyId)
        {
            bool isReported = await _context.Notifications.AnyAsync(n => n.StoryID == storyId && n.IsRead == false && n.Type == PBL3.Models.NotificationModel.NotificationType.ReportStory);
            bool chaptersReported = await _context.Notifications.AnyAsync(n => n.StoryID == storyId && n.IsRead == false && n.Type == PBL3.Models.NotificationModel.NotificationType.ReportChapter);
            return isReported || chaptersReported;
        }

        private async Task<ViewModels.Moderator.UserProfileViewModel> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new ViewModels.Moderator.UserProfileViewModel
                {
                    UserID = u.UserID,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    Status = u.Status
                })
                .FirstOrDefaultAsync();

            if (user != null && !string.IsNullOrEmpty(user.Avatar))
            {
                user.Avatar = await _blobService.GetSafeImageUrlAsync(user.Avatar);
            }

            return user;
        }

        private async Task<List<CommentViewModel>> GetUserCommentsAsync(int userId)
        {
            var comments = await _context.Comments
                .Where(c => c.UserID == userId)
                .Select(c => new CommentViewModel
                {
                    CommentID = c.CommentID,
                    UserID = c.UserID,
                    ChapterID = c.ChapterID,
                    StoryID = c.StoryID,
                    Content = c.Content,
                    IsDeleted = c.isDeleted,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return comments;
        }
        private async Task<List<ReportViewModel>> GetReportsCreatedByUserAsync(int userId)
        {
            var reports = await _notificationService.GetReportNotificationsAsync();
            return reports
                .Where(r => r.FromUserID == userId)
                .Select(r => new ReportViewModel
                {
                    ReportID = r.NotificationID,
                    ReportedUserID = r.UserID,
                    ReporterID = r.FromUserID,
                    StoryID = r.StoryID,
                    ChapterID = r.ChapterID,
                    CommentID = r.CommentID,
                    Reason = r.Message,
                    ReportType = r.Type,
                    CreatedAt = r.CreatedAt,
                    Status = r.IsRead
                })
                .ToList();
        }
        private async Task<List<ReportViewModel>> GetReportsReceivedByUserAsync(int userId)
        {
            var reports = await _notificationService.GetReportNotificationsAsync();
            return reports
                .Where(r => r.UserID == userId)
                .Select(r => new ReportViewModel
                {
                    ReportID = r.NotificationID,
                    ReportedUserID = r.UserID,
                    ReporterID = r.FromUserID,
                    StoryID = r.StoryID,
                    ChapterID = r.ChapterID,
                    CommentID = r.CommentID,
                    Reason = r.Message,
                    ReportType = r.Type,
                    CreatedAt = r.CreatedAt,
                    Status = r.IsRead
                })
                .ToList();
        }
        private async Task<List<UserStoryCardViewModel>> GetUserStoriesAsync(int userId)
        {
            var userStories = await _context.Stories
                .Where(s => s.AuthorID == userId && s.Status != PBL3.Models.StoryModel.StoryStatus.Inactive)
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
            foreach (var story in userStories)
            {
                story.Cover = await _blobService.GetSafeImageUrlAsync(story.Cover);
            }
            return userStories;
        }
    }
}
