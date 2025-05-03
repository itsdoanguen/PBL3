using static PBL3.Models.StoryModel;

namespace PBL3.ViewModels.Story
{
    public class StoryDetailViewModel
    {
        public int StoryID { get; set; }
        public string StoryName { get; set; }
        public string StoryDescription { get; set; }
        public string StoryImage { get; set; }
        public List<GerneVM> gerneVMs { get; set; } = new List<GerneVM>();
        public StoryStatus StoryStatus { get; set; }

        public DateTime? LastUpdated { get; set; }
        public double Rating { get; set; }
        public int TotalBookmark { get; set; }
        public int TotalChapter { get; set; }
        public int TotalWord { get; set; }
        public int TotalComment { get; set; }
        public int TotalView { get; set; }
        public int TotalFollow { get; set; }

        public UserInfo Author { get; set; } = new UserInfo();
        public bool IsFollowed { get; set; } = false;

        public List<CommentInfo> Comments { get; set; } = new List<CommentInfo>();
        public List<ChapterInfo> Chapters { get; set; } = new List<ChapterInfo>();
    }

    public class CommentInfo
    {
        public int CommentID { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class ChapterInfo
    {
        public int ChapterID { get; set; }
        public string Title { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserInfo
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
    }
}
