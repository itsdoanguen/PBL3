using PBL3.Models;
using PBL3.ViewModels.Comment;

namespace PBL3.ViewModels.Chapter
{
    public class ChapterDetailViewModel
    {
        public int ChapterID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ViewCount { get; set; }
        public int TotalWord { get; set; }

        public string StoryTitle { get; set; }
        public int StoryID { get; set; }

        public bool IsLikedByCurrentUser { get; set; }
        public int LikeCount { get; set; }
        public List<CommentTreeViewModel> Comments { get; set; }

        public int NextChapterID { get; set; }
        public int PreviousChapterID { get; set; }

        public List<ChapterList> ChapterList = new List<ChapterList>();
        public StyleViewModel Style { get; set; } = new StyleViewModel();
    }

    public class ChapterList
    {
        public int ChapterID { get; set; }
        public string Title { get; set; }
    }
    public class StyleViewModel
    {
        public int StyleID { get; set; }
        public int UserID { get; set; }
        public FontFamily FontFamily { get; set; } = FontFamily.Arial;
        public int FontSize { get; set; } = 16;
        public BackgroundColor BackgroundColor { get; set; } = BackgroundColor.White;
        public string TextColorHex => BackgroundColor switch
        {
            BackgroundColor.Black => "#EEEEEE",
            _ => "#1A1A1A"
        };
        public string BackgroundColorHex => BackgroundColor switch
        {
            BackgroundColor.White => "#FDFDFD",
            BackgroundColor.Black => "#121212",
            BackgroundColor.Blue => "#DDEEFF",
            BackgroundColor.Yellow => "#FFF9DC",
            BackgroundColor.Pink => "#FFE4F1",
            BackgroundColor.Gray => "#ECECEC",
            _ => "#FFFFFF"
        };
    }
    public class CommentTreeViewModel
    {
        public CommentPostViewModel Comment { get; set; } = null;
        public List<CommentTreeViewModel> Replies { get; set; } = new List<CommentTreeViewModel>();
    }
}
