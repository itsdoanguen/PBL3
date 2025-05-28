namespace PBL3.ViewModels.FollowStory
{
    public class FollowStoryItemsViewModel
    {
        public int StoryID { get; set; }
        public string StoryTitle { get; set; }
        public string StoryCoverImageUrl { get; set; } // nếu có ảnh bìa
    }

    public class FollowStoryViewModel
    {
        public int UserID { get; set; }
        public List<FollowStoryItemsViewModel> FollowedStories { get; set; } = new List<FollowStoryItemsViewModel>();
    }
}
