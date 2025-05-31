using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;

namespace PBL3.Service.Report
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }
        // Tạo noti khi có report user
        public async Task InitReportUserNotificationAsync(int reportedUserId, int fromUserId, string message)
        {
            var noti = new NotificationModel
            {
                UserID = reportedUserId,
                Type = NotificationModel.NotificationType.ReportUser,
                Message = message,
                FromUserID = fromUserId
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }
        // Tạo noti khi có report comment
        public async Task InitReportCommentNotificationAsync(int commentId, int fromUserId, string message)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return;
            var noti = new NotificationModel
            {
                UserID = comment.UserID,
                Type = NotificationModel.NotificationType.ReportComment,
                Message = message,
                FromUserID = fromUserId,
                CommentID = commentId,
                ChapterID = comment.ChapterID,
                StoryID = comment.StoryID
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }
        // Tạo noti khi có report chapter
        public async Task InitReportChapterNotificationAsync(int chapterId, int fromUserId, string message)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter == null) return;
            var story = await _context.Stories.FindAsync(chapter.StoryID);
            if (story == null) return;
            var noti = new NotificationModel
            {
                UserID = story.AuthorID,
                Type = NotificationModel.NotificationType.ReportChapter,
                Message = message,
                FromUserID = fromUserId,
                ChapterID = chapterId,
                StoryID = story.StoryID
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }
        // Tạo noti khi có report story
        public async Task InitReportStoryNotificationAsync(int storyId, int fromUserId, string message)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null) return;
            var noti = new NotificationModel
            {
                UserID = story.AuthorID,
                Type = NotificationModel.NotificationType.ReportStory,
                Message = message,
                FromUserID = fromUserId,
                StoryID = storyId
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }
        // Lấy danh sách noti report cho moderator
        public async Task<List<NotificationModel>> GetReportNotificationsAsync()
        {
            return await _context.Notifications
                .Where(n =>
                    n.Type == NotificationModel.NotificationType.ReportUser ||
                    n.Type == NotificationModel.NotificationType.ReportComment ||
                    n.Type == NotificationModel.NotificationType.ReportChapter ||
                    n.Type == NotificationModel.NotificationType.ReportStory)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
