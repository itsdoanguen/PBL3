using System.Collections.Generic;

namespace PBL3.ViewModels.LikeChapter
{
    public class LikedChapterItemViewModel
    {
        public int ChapterID { get; set; }
        public string ChapterTitle { get; set; }
        public int StoryID { get; set; }
        public string StoryTitle { get; set; }
        public string? StoryCoverImageUrl { get; set; }
    }

    public class LikedChapterViewModel
    {
        public List<LikedChapterItemViewModel> LikedChapters { get; set; } = new();
    }
}
