using System.ComponentModel.DataAnnotations;
using PBL3.Models;
using PBL3.ViewModels.Chapter;

namespace PBL3.ViewModels.Story
{
    public class StoryEditViewModel
    {
        public int StoryID { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CoverImage { get; set; }

        public int TotalLike { get; set; }
        public int TotalBookmark { get; set; }
        public int TotalChapter { get; set; }
        public int TotalComment { get; set; }
        public int TotalView { get; set; }
        public ICollection<ChapterSummaryViewModel>? Chapters { get; set; }
    }
}
