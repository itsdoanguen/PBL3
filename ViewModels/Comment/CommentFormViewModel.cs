namespace PBL3.ViewModels.Chapter
{
    public class CommentFormViewModel
    {
        public int ChapterID { get; set; }
        public string UserID { get; set; }
        public int? ParentCommentID { get; set; }
        public string Type { get; set; } = "chapter";
        public string ReplyingToUsername { get; set; }
        public string FormId { get; set; } = "main";
    }
}