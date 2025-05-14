namespace PBL3.ViewModels.Story
{
    public class CommentFormViewModel
    {
        public int StoryID { get; set; }
        public string UserID { get; set; }
        public int? ParentCommentID { get; set; }
        public string Type { get; set; } = "story";
        public string ReplyingToUsername { get; set; }
        public string FormId { get; set; } = "main";
    }
}
