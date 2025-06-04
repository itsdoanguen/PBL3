using PBL3.Models;

namespace PBL3.ViewModels.UserProfile
{
    public class UserStoryCardViewModel
    {
        public int StoryID { get; set; }
        public string Title { get; set; }
        public string? Cover { get; set; }
        public int TotalChapters { get; set; }
        public DateTime? LastUpdated { get; set; }
        public StoryModel.StoryStatus Status { get; set; }

        public string? Discription { get; set; }
        public int? TotalViews { get; set; }
        public int? TotalLikes { get; set; }
        public int? TotalWords { get; set; }
        public int? TotalFollowers { get; set; }
    }
}
