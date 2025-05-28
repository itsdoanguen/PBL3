using PBL3.Models;

namespace PBL3.ViewModels.Moderator
{
    public class ViewStoryViewModel
    {
        public UserProfileViewModel Author { get; set; } = new UserProfileViewModel();
        public M_StoryViewModel StoryDetails { get; set; } = new M_StoryViewModel();
        public List<M_ChapterViewModel> Chapters { get; set; } = new List<M_ChapterViewModel>();
    }
    public class M_StoryViewModel
    {
        public int StoryID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public string Description { get; set; } = string.Empty;
        public StoryModel.StoryStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public bool isReported { get; set; } = false;
    }
    public class M_ChapterViewModel
    {
        public int ChapterID { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ChapterOrder { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public ChapterStatus Status { get; set; } = ChapterStatus.Inactive;
        public int ViewCount { get; set; } = 0;
        public bool isReported { get; set; } = false;
    }
}