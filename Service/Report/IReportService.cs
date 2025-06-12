using PBL3.Models;

namespace PBL3.Service.Report
{
    public interface IReportService
    {
        Task InitReportUserNotificationAsync(int reportedUserId, int fromUserId, string message);
        Task InitReportCommentNotificationAsync(int commentId, int fromUserId, string message);
        Task InitReportChapterNotificationAsync(int chapterId, int fromUserId, string message);
        Task InitReportStoryNotificationAsync(int storyId, int fromUserId, string message);
        Task<List<NotificationModel>> GetReportNotificationsAsync();
    }
}
