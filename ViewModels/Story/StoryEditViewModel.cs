using PBL3.ViewModels.Chapter;
using static PBL3.Models.StoryModel;

namespace PBL3.ViewModels.Story
{
    public class StoryEditViewModel
    {
        public int StoryID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public StoryStatus StoryStatus { get; set; } = StoryStatus.Inactive;

        public int TotalLike { get; set; }
        public int TotalBookmark { get; set; }
        public int TotalChapter { get; set; }
        public int TotalComment { get; set; }
        public int TotalView { get; set; }
        public ICollection<ChapterSummaryViewModel>? Chapters { get; set; }
    }
}
