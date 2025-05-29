using Microsoft.Data.SqlClient;
using PBL3.ViewModels.UserProfile;
using static PBL3.Models.NotificationModel;
using static PBL3.Models.UserModel;

namespace PBL3.ViewModels.Moderator
{
    public class ViewUserViewModel
    {
        public UserProfileViewModel userProfile { get; set; } = new UserProfileViewModel();
        public List<UserStoryCardViewModel> userStories { get; set; } = new List<UserStoryCardViewModel>();
        public List<CommentViewModel> UserComments { get; set; } = new List<CommentViewModel>();
        public List<ReportViewModel> ReportsCreated { get; set; } = new List<ReportViewModel>();
        public List<ReportViewModel> ReportsReceived { get; set; } = new List<ReportViewModel>();
    }

    public class UserProfileViewModel
    {
        public int UserID { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserStatus Status { get; set; }
        public int TotalWarning { get; set; }
    }
    public class CommentViewModel
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public int? ChapterID { get; set; }
        public int? StoryID { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class ReportViewModel
    {
        public int ReportID { get; set; }
        public int? ReporterID { get; set; }
        public int? ReportedUserID { get; set; }
        public int? StoryID { get; set; }
        public int? ChapterID { get; set; }
        public int? CommentID { get; set; }
        public NotificationType ReportType { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }
    }
}