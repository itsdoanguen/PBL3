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
                .Where(s => s.AuthorID == userId)
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
