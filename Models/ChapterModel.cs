using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class ChapterModel
    {
        [Key]
        public int ChapterID { get; set; }
        [ForeignKey("Story")]
        public int StoryID { get; set; }
        [Required, StringLength(255)]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public ChapterStatus Status { get; set; } = ChapterStatus.Inactive;
        public int ViewCount { get; set; } = 0;

        // Navigation properties
        public StoryModel? Story { get; set; }
        public ICollection<CommentModel> Comments { get; set; } 
        public ICollection<LikeChapterModel> Likes { get; set; }
        public ICollection<BookmarkModel> Bookmarks { get; set; }

    }

    public enum ChapterStatus
    {
        Active,
        Inactive
    }
}
