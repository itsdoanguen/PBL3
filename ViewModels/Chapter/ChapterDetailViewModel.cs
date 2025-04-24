using PBL3.Models;

namespace PBL3.ViewModels.Chapter
{
    public class ChapterDetailViewModel
    {
        public int ChapterID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ViewCount { get; set; }

        public string StoryTitle { get; set; }
        public int StoryID { get; set; }

        public List<CommentModel> Comments { get; set; }
    }

}
