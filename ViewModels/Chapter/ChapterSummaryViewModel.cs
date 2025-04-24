namespace PBL3.ViewModels.Chapter
{
    public class ChapterSummaryViewModel
    {
        public int ChapterID { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ChapterStatus Status { get; set; } = ChapterStatus.Inactive;
        public int ViewCount { get; set; }
        public int ChapterOrder { get; set; }

        public enum ChapterStatus
        {
            Active,
            Inactive
        }
    }
}
