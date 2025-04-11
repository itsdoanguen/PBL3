namespace PBL3.ViewModels.Chapter
{
    public class ChapterSummaryViewModel
    {
        public int ChapterID { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ViewCount { get; set; }
    }
}
