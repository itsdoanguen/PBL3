using System.Threading.Tasks;
using PBL3.Models;
using System.Collections.Generic;

namespace PBL3.Service.Notification
{
    public interface INotificationService
    {
        Task InitNewChapterNotificationAsync(int storyId, int chapterId, int fromUserId);
        Task InitNewStoryNotificationAsync(int storyId, int fromUserId);
        Task InitNewReplyCommentNotificationAsync(int commentId, int fromUserId);
        Task InitNewCommentNotificationAsync(int storyId, int commentId, int fromUserId);
        Task InitNewFollowNotificationAsync(int followerId, int followingId);
        Task InitNewMessageFromModeratorAsync(int userId, string message, int moderatorId);
        Task<List<NotificationModel>> GetNotificationsForUserAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task<(bool isSuccess, string message)> DeleteNotificationAsync(int notificationId);        Task<NotificationModel?> GetNotificationByIdAsync(int notificationId);
    }
}
