using PBL3.Models;

namespace PBL3.ViewModels.UserProfile
{
    public class UserStoryCardViewModel
    {
        public int StoryID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Cover { get; set; }
        public int TotalChapters { get; set; }
        public DateTime? LastUpdated { get; set; }
        public StoryModel.StoryStatus Status { get; set; }

        public string? Discription { get; set; }
        public int? TotalViews { get; set; }
        public int? TotalLikes { get; set; }
        public int? TotalWords { get; set; }
        public int? TotalFollowers { get; set; }

        // Additional properties for better display compatibility
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Computed properties for view compatibility
        public int Id => StoryID;
        public string CoverImageUrl => Cover ?? "/image/default-cover.jpg";
        public int ViewCount => TotalViews ?? 0;
        public int LikeCount => TotalLikes ?? 0;
        public int ChapterCount => TotalChapters;
        public int FollowCount => TotalFollowers ?? 0;
        
        // Status-based computed properties
        public bool IsHot => ViewCount > 1000 || (TotalLikes ?? 0) > 50;
        public bool IsNew => CreatedAt > DateTime.Now.AddDays(-7);
        public bool IsCompleted => Status == StoryModel.StoryStatus.Completed;
    }
}
