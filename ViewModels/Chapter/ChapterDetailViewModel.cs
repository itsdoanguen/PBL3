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

        public bool IsLikedByCurrentUser { get; set; } 
        public int LikeCount { get; set; } 
        public List<CommentModel> Comments { get; set; }

        public int NextChapterID { get; set; }
        public int PreviousChapterID { get; set; } 

        public List<ChapterList> ChapterList = new List<ChapterList>();
    }

    public class ChapterList { 
        public int ChapterID { get; set; }
        public string Title { get; set; }
    }

}
