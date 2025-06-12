using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Admin;

namespace PBL3.Service.Admin
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            var users = _context.Users.AsQueryable();
            var stories = _context.Stories.AsQueryable();
            var chapters = _context.Chapters.AsQueryable();
            var comments = _context.Comments.AsQueryable();
            var reports = _context.Notifications.AsQueryable();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await users.CountAsync(),
                TotalStories = await stories.CountAsync(),
                TotalChapters = await chapters.CountAsync(),
                TotalComments = await comments.CountAsync(),
                TotalReports = await reports.CountAsync(),
                TotalAdmins = await users.CountAsync(u => u.Role == UserModel.UserRole.Admin),
                TotalModerators = await users.CountAsync(u => u.Role == UserModel.UserRole.Moderator),
                TotalNormalUsers = await users.CountAsync(u => u.Role == UserModel.UserRole.User),
                ActiveStories = await stories.CountAsync(s => s.Status == StoryModel.StoryStatus.Active),
                LockedStories = await stories.CountAsync(s => s.Status == StoryModel.StoryStatus.Locked),
                ReviewPendingStories = await stories.CountAsync(s => s.Status == StoryModel.StoryStatus.ReviewPending),
                CompletedStories = await stories.CountAsync(s => s.Status == StoryModel.StoryStatus.Completed),
                NewUsersLast7Days = await users.CountAsync(u => u.CreatedAt >= DateTime.Now.AddDays(-7))
            };
            return model;
        }

        public async Task<AdminReportViewModel> GetReportStatsAsync(DateTime? from = null, DateTime? to = null, int? tagId = null)
        {
            var users = _context.Users.AsQueryable();
            var stories = _context.Stories.AsQueryable();
            var genres = _context.Genres.AsQueryable();
            var storyGenres = _context.StoryGenres.AsQueryable();
            var notis = _context.Notifications.AsQueryable();

            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                users = users.Where(u => u.CreatedAt >= fromDate);
                stories = stories.Where(s => s.CreatedAt >= fromDate);
                notis = notis.Where(n => n.CreatedAt >= fromDate);
            }
            if (to.HasValue)
            {
                // Lấy hết ngày to (23:59:59.999)
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                users = users.Where(u => u.CreatedAt <= toDate);
                stories = stories.Where(s => s.CreatedAt <= toDate);
                notis = notis.Where(n => n.CreatedAt <= toDate);
            }

            // Generate full date range
            var startDate = from?.Date ?? DateTime.Now.Date.AddDays(-7);
            var endDate = (to?.Date ?? DateTime.Now.Date).AddDays(1);
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset))
                .ToList();

            // User per day 
            var userRaw = await users
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();
            var newUsersPerDay = allDates
                .Select(d => new UserPerDay { Date = d, Count = userRaw.FirstOrDefault(x => x.Date == d)?.Count ?? 0 })
                .ToList();
            int totalNewUsers = newUsersPerDay.Sum(x => x.Count);

            // Story per day 
            var filteredStories = stories;
            if (tagId.HasValue)
            {
                var storyIds = await storyGenres.Where(sg => sg.GenreID == tagId.Value).Select(sg => sg.StoryID).ToListAsync();
                filteredStories = filteredStories.Where(s => storyIds.Contains(s.StoryID));
            }
            var storyRaw = await filteredStories
                .GroupBy(s => s.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();
            var newStoriesPerDay = allDates
                .Select(d => new StoryPerDay { Date = d, Count = storyRaw.FirstOrDefault(x => x.Date == d)?.Count ?? 0 })
                .ToList();
            int totalNewStories = newStoriesPerDay.Sum(x => x.Count);

            // Tag list
            var availableTags = await genres.Select(g => new GenreTagItem { Id = g.GenreID, Name = g.Name }).ToListAsync();
            int newStoriesByTag = 0;
            if (tagId.HasValue)
            {
                newStoriesByTag = await storyGenres.CountAsync(sg => sg.GenreID == tagId.Value && stories.Any(s => s.StoryID == sg.StoryID));
            }

            // Report (notification) per day
            var reportTypes = new[] {
                NotificationModel.NotificationType.ReportUser,
                NotificationModel.NotificationType.ReportComment,
                NotificationModel.NotificationType.ReportChapter,
                NotificationModel.NotificationType.ReportStory
            };
            var reportNotis = notis.Where(n => reportTypes.Contains(n.Type));
            var reportRaw = await reportNotis
                .GroupBy(n => n.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();
            var reportsPerDay = allDates
                .Select(d => new ReportPerDay { Date = d, Count = reportRaw.FirstOrDefault(x => x.Date == d)?.Count ?? 0 })
                .ToList();
            int totalReports = reportsPerDay.Sum(x => x.Count);
            var reportTypeCounts = await reportNotis
                .GroupBy(n => n.Type)
                .Select(g => new ReportTypeCount { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var model = new AdminReportViewModel
            {
                TotalNewUsers = totalNewUsers,
                NewUsersPerDay = newUsersPerDay,
                TotalNewStories = totalNewStories,
                NewStoriesPerDay = newStoriesPerDay,
                AvailableTags = availableTags,
                SelectedTagId = tagId,
                NewStoriesByTag = newStoriesByTag,
                StartDate = from,
                EndDate = to,
                // Thêm phần report
                TotalReports = totalReports,
                ReportsPerDay = reportsPerDay,
                ReportTypeCounts = reportTypeCounts
            };
            return model;
        }

        public async Task<AdminManageSystemViewModel> GetManageSystemStatsAsync()
        {
            // Lấy danh sách user
            var users = await _context.Users.Where(u => u.Role != UserModel.UserRole.Admin)
                .Select(u => new UserInfo
                {
                    Id = u.UserID,
                    UserName = u.DisplayName ?? "[]",
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    TotalWarnings = u.TotalWarning,
                    IsActive = u.Status == UserModel.UserStatus.Active
                })
                .ToListAsync();

            // Lấy danh sách truyện đang hoạt động
            var activeStories = await _context.Stories
                .Where(s => s.Status == StoryModel.StoryStatus.Active)
                .Select(s => new StoryInfo
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    Author = s.Author != null ? s.Author.DisplayName ?? "[]" : "[]",
                    ChapterCount = _context.Chapters.Count(c => c.StoryID == s.StoryID),
                    Status = s.Status.ToString()
                })
                .ToListAsync();

            // Lấy danh sách truyện đã hoàn thành
            var completedStories = await _context.Stories
                .Where(s => s.Status == StoryModel.StoryStatus.Completed)
                .Select(s => new StoryInfo
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    Author = s.Author != null ? s.Author.DisplayName ?? "[]" : "[]",
                    ChapterCount = _context.Chapters.Count(c => c.StoryID == s.StoryID),
                    Status = s.Status.ToString()
                })
                .ToListAsync();

            // Lấy danh sách truyện bị khóa
            var lockedStories = await _context.Stories
                .Where(s => s.Status == StoryModel.StoryStatus.Locked)
                .Select(s => new StoryInfo
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    Author = s.Author != null ? s.Author.DisplayName ?? "[]" : "[]",
                    ChapterCount = _context.Chapters.Count(c => c.StoryID == s.StoryID),
                    Status = s.Status.ToString()
                })
                .ToListAsync();

            // Lấy danh sách truyện chờ duyệt
            var reviewPendingStories = await _context.Stories
                .Where(s => s.Status == StoryModel.StoryStatus.ReviewPending)
                .Select(s => new StoryInfo
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    Author = s.Author != null ? s.Author.DisplayName ?? "[]" : "[]",
                    ChapterCount = _context.Chapters.Count(c => c.StoryID == s.StoryID),
                    Status = s.Status.ToString()
                })
                .ToListAsync();

            // Lấy danh sách thể loại
            var genres = await _context.Genres
                .Select(g => new GenreInfo
                {
                    Id = g.GenreID,
                    Name = g.Name,
                    StoryCount = _context.StoryGenres.Count(sg => sg.GenreID == g.GenreID)
                })
                .ToListAsync();

            return new AdminManageSystemViewModel
            {
                Users = users,
                ActiveStories = activeStories,
                CompletedStories = completedStories,
                LockedStories = lockedStories,
                ReviewPendingStories = reviewPendingStories,
                Genres = genres
            };
        }
    }
}
