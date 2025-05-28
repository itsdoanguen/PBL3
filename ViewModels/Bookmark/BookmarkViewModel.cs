namespace PBL3.ViewModels.Bookmark
{
    public class BookmarkItemViewModel
    {
        public int StoryID { get; set; }
        public string StoryTitle { get; set; }
        public string StoryCoverImageUrl { get; set; } // nếu có ảnh bìa
        public int ChapterID { get; set; }
        public string ChapterTitle { get; set; }
    }

    public class BookmarkViewModel
    {
        public int UserID { get; set; }
        public List<BookmarkItemViewModel> BookmarkedStories { get; set; } = new List<BookmarkItemViewModel>();
    }
}
