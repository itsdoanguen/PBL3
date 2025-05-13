using System;
using System.ComponentModel.DataAnnotations;

namespace PBL3.Models
{
    public class UserModel
    {
        [Key]
        public int UserID { get; set; }
        [Required,StringLength(255),EmailAddress]
        public string Email { get; set; }
        [Required,StringLength(255)]
        public string PasswordHash { get; set; }
        [StringLength(100)]
        public string? DisplayName { get; set; }
        [StringLength(255)]
        public string? Avatar { get; set; }
        [StringLength(255)]
        public string? Banner { get; set; }
        [StringLength(1000)]
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? DateOfBirth { get; set; }
        [Required]
        public UserRole Role { get; set; } = UserRole.User;
        [Required]
        public UserStatus Status { get; set; } = UserStatus.Active;
        public UserGender? Gender { get; set; }

        // Navigation properties
        public StyleModel? Style { get; set; }
        public ICollection<StoryModel> Stories { get; set; }
        public ICollection<CommentModel> Comments { get; set; }
        public ICollection<NotificationModel> Notifications { get; set; }
        public ICollection<BookmarkModel> Bookmarks { get; set; }
        public ICollection<FollowUserModel> Followers { get; set; }
        public ICollection<FollowUserModel> Followings { get; set; }
        public ICollection<FollowStoryModel> FollowStories { get; set; }
        public ICollection<LikeChapterModel> LikeChapters { get; set; }



        //Enum 
        public enum UserRole
        {
            User,
            Moderator,
            Admin
        }
        public enum UserStatus
        {
            Active,
            Banned
        }
        public enum UserGender
        {
            Male,
            Female,
            Other
        }
    }
}
