using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;

namespace PBL3.Service.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tạo noti khi có chapter mới cho story mà user follow
        public async Task InitNewChapterNotificationAsync(int storyId, int chapterId, int fromUserId)
        {
            var followers = await _context.FollowStories
                .Where(f => f.StoryID == storyId && f.UserID != fromUserId)
                .Select(f => f.UserID)
                .ToListAsync();

            var story = await _context.Stories.FindAsync(storyId);
            var chapter = await _context.Chapters.FindAsync(chapterId);

            foreach (var userId in followers)
            {
                var noti = new NotificationModel
                {
                    UserID = userId,
                    Type = NotificationModel.NotificationType.NewChapter,
                    Message = $"Truyện '{story?.Title}' vừa có chương mới: '{chapter?.Title}'",
                    StoryID = storyId,
                    ChapterID = chapterId,
                    FromUserID = fromUserId
                };
                _context.Notifications.Add(noti);
            }
            await _context.SaveChangesAsync();
        }

        // Tạo noti khi user mà mình follow đăng truyện mới
        public async Task InitNewStoryNotificationAsync(int storyId, int fromUserId)
        {
            var followers = await _context.FollowUsers
                .Where(f => f.FollowingID == fromUserId && f.FollowerID != fromUserId)
                .Select(f => f.FollowerID)
                .ToListAsync();

            var story = await _context.Stories.FindAsync(storyId);
            var fromUser = await _context.Users.FindAsync(fromUserId);

            foreach (var userId in followers)
            {
                var noti = new NotificationModel
                {
                    UserID = userId,
                    Type = NotificationModel.NotificationType.NewStory,
                    Message = $"{fromUser?.DisplayName ?? "Người bạn theo dõi"} vừa đăng truyện mới: '{story?.Title}'",
                    StoryID = storyId,
                    FromUserID = fromUserId
                };
                _context.Notifications.Add(noti);
            }
            await _context.SaveChangesAsync();
        }

        // Tạo noti khi có người reply comment của mình
        public async Task InitNewReplyCommentNotificationAsync(int commentId, int fromUserId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.ParentCommentID == null)
                return;
            var parentComment = await _context.Comments.FindAsync(comment.ParentCommentID);
            if (parentComment == null || parentComment.UserID == fromUserId)
                return;
            var noti = new NotificationModel
            {
                UserID = parentComment.UserID,
                Type = NotificationModel.NotificationType.NewReplyComment,
                Message = $"{(await _context.Users.FindAsync(fromUserId))?.DisplayName ?? "Ai đó"} đã trả lời bình luận của bạn.",
                CommentID = commentId,
                FromUserID = fromUserId
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }

        // Tạo noti khi có người comment trên truyện của mình
        public async Task InitNewCommentNotificationAsync(int storyId, int commentId, int fromUserId)
        {
            var story = await _context.Stories.FindAsync(storyId);
            if (story == null || story.AuthorID == fromUserId)
                return;
            var noti = new NotificationModel
            {
                UserID = story.AuthorID,
                Type = NotificationModel.NotificationType.NewComment,
                Message = $"{(await _context.Users.FindAsync(fromUserId))?.DisplayName ?? "Ai đó"} đã bình luận trên truyện của bạn.",
                StoryID = storyId,
                CommentID = commentId,
                FromUserID = fromUserId
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }

        // Tạo noti khi có người follow mình
        public async Task InitNewFollowNotificationAsync(int followerId, int followingId)
        {
            if (followerId == followingId) return;
            var follower = await _context.Users.FindAsync(followerId);
            var noti = new NotificationModel
            {
                UserID = followingId,
                Type = NotificationModel.NotificationType.NewFollower,
                Message = $"{follower?.DisplayName ?? "Ai đó"} đã theo dõi bạn.",
                FromUserID = followerId
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }
        //Tạo noti khi có feedback từ moderator
        public async Task InitNewMessageFromModeratorAsync(int userId, string message, int moderatorId)
        {
            var noti = new NotificationModel
            {
                UserID = userId,
                Type = NotificationModel.NotificationType.NewStory,
                Message = message,
                FromUserID = moderatorId
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }
        public async Task InitNewWarningMessageAsync(int userId, string message, int moderatorId)
        {
            var noti = new NotificationModel
            {
                UserID = userId,
                Type = NotificationModel.NotificationType.WarningIssued,
                Message = "Thông báo: Bạn đã bị cảnh cáo vì hành vi của mình. \n Note: " + message,
                FromUserID = moderatorId
            };
            _context.Notifications.Add(noti);
            await _context.SaveChangesAsync();
        }

        // Đánh dấu đã đọc
        public async Task MarkAsReadAsync(int notificationId)
        {
            var noti = await _context.Notifications.FindAsync(notificationId);
            if (noti != null && !noti.IsRead)
            {
                noti.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(bool isSuccess, string message)> DeleteNotificationAsync(int notificationId)
        {
            var noti = await _context.Notifications.FindAsync(notificationId);
            if (noti != null)
            {
                _context.Notifications.Remove(noti);
                await _context.SaveChangesAsync();
                return (true, "Xóa thông báo thành công!");
            }
            return (false, "Không tìm thấy thông báo để xóa!");
        }

        // Lấy noti theo ID
        public async Task<NotificationModel?> GetNotificationByIdAsync(int notificationId)
        {
            return await _context.Notifications.FindAsync(notificationId);
        }

        // Lấy danh sách thông báo cho user
        public async Task<List<NotificationModel>> GetNotificationsForUserAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserID == userId && (n.Type == NotificationModel.NotificationType.NewStory || n.Type == NotificationModel.NotificationType.NewChapter
                    || n.Type == NotificationModel.NotificationType.NewComment
                    || n.Type == NotificationModel.NotificationType.NewReplyComment
                    || n.Type == NotificationModel.NotificationType.NewFollower
                    || n.Type == NotificationModel.NotificationType.WarningIssued))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // Lấy số lượng thông báo chưa đọc cho user
        public async Task<int> GetUnreadNotificationCountAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserID == userId && !n.IsRead && (n.Type == NotificationModel.NotificationType.NewStory || n.Type == NotificationModel.NotificationType.NewChapter
                    || n.Type == NotificationModel.NotificationType.NewComment
                    || n.Type == NotificationModel.NotificationType.NewReplyComment
                    || n.Type == NotificationModel.NotificationType.NewFollower
                    || n.Type == NotificationModel.NotificationType.WarningIssued))
                .CountAsync();
        }
    }
}
