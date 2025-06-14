﻿namespace PBL3.ViewModels.Comment
{
    public class CommentPostViewModel
    {
        public int CommentID { get; set; }
        public int UserID { get; set; }
        public int? ChapterID { get; set; }
        public int? StoryID { get; set; }
        public int? ParentCommentID { get; set; } = null;
        public string? ParentUserName { get; set; } = null;
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public bool isDeleted { get; set; } = false;

        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }  
        public int RepliesCount { get; set; } = 0;
    }
}
