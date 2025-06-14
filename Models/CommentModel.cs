﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class CommentModel
    {
        [Key]
        public int CommentID { get; set; }
        [ForeignKey("UserID")]
        public int UserID { get; set; }

        [ForeignKey("Chapter")]
        public int? ChapterID { get; set; }
        [ForeignKey("Story")]
        public int? StoryID { get; set; }
        public int? ParentCommentID { get; set; } = null;
        [Required]
        public string Content { get; set; }
        public bool isDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public UserModel? User { get; set; }
        public ChapterModel? Chapter { get; set; }
        public StoryModel? Story { get; set; }

    }
}
