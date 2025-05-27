using System.Threading.Tasks;
using PBL3.Models;
using System.Collections.Generic;

namespace PBL3.Service.Notification
{
    public interface INotificationService
    {
        // Tạo noti khi có chapter mới cho story mà user follow
        Task InitNewChapterNotificationAsync(int storyId, int chapterId, int fromUserId);
        // Tạo noti khi user mà mình follow đăng truyện mới
        Task InitNewStoryNotificationAsync(int storyId, int fromUserId);
        // Tạo noti khi có người reply comment của mình
        Task InitNewReplyCommentNotificationAsync(int commentId, int fromUserId);
        // Tạo noti khi có người comment trên truyện của mình
        Task InitNewCommentNotificationAsync(int storyId, int commentId, int fromUserId);
        // Tạo noti khi có người follow mình
        Task InitNewFollowNotificationAsync(int followerId, int followingId);
        // Lấy danh sách noti của user
        Task<List<NotificationModel>> GetNotificationsForUserAsync(int userId);
        // Đánh dấu đã đọc
        Task MarkAsReadAsync(int notificationId);
        // Xóa thông báo, trả về trạng thái và message
        Task<(bool isSuccess, string message)> DeleteNotificationAsync(int notificationId);
        // Tạo noti khi có report user
        Task InitReportUserNotificationAsync(int reportedUserId, int fromUserId, string message);
        // Tạo noti khi có report comment
        Task InitReportCommentNotificationAsync(int commentId, int fromUserId, string message);
        // Tạo noti khi có report chapter
        Task InitReportChapterNotificationAsync(int chapterId, int fromUserId, string message);
        // Tạo noti khi có report story
        Task InitReportStoryNotificationAsync(int storyId, int fromUserId, string message);
        // Lấy danh sách noti report cho moderator
        Task<List<NotificationModel>> GetReportNotificationsAsync();
        // Lấy noti theo ID
        Task<NotificationModel?> GetNotificationByIdAsync(int notificationId);
    }
}
