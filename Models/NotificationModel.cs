using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class NotificationModel
    {
        [Key]
        public int NotificationID { get; set; }
        [ForeignKey("UserID")]
        public int UserID { get; set; }
        [Required]
        public NotificationType Type { get; set; }
        [Required, StringLength(255)]
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liên kết đối tượng phát sinh noti
        public int? StoryID { get; set; }
        public int? ChapterID { get; set; }
        public int? CommentID { get; set; }
        public int? FromUserID { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public UserModel? User { get; set; }
        public UserModel? FromUser { get; set; }
        public StoryModel? Story { get; set; }
        public ChapterModel? Chapter { get; set; }
        public CommentModel? Comment { get; set; }

        public enum NotificationType
        {
            NewComment,
            NewFollower,
            NewChapter,
            NewStory,
            NewReplyComment,
            ReportUser,
            ReportComment,
            ReportChapter,
            ReportStory
        }
    }
}
