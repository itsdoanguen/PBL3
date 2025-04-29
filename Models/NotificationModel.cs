using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class NotificationModel
    {
        [Key]
        public int NotificationID { get; set; }
        [ForeignKey("User")]
        public int UserID { get; set; }
        [Required, StringLength(50)]
        public NotificationType Title { get; set; }
        [Required, StringLength(255)]
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public UserModel? User { get; set; }


        public enum NotificationType
        {
            NewComment,
            NewFollower,
            NewChapter
        }
    }
}
