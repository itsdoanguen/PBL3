using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class StoryModel
    {
        [Key]
        public int StoryID { get; set; }
        [Required,StringLength(255)]
        public string Title { get; set; }
        [Required,StringLength(500)]
        public string Description { get; set; }
        public string? CoverImage { get; set; }
        public StoryStatus Status { get; set; } = StoryStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("Author")]
        public int AuthorID { get; set; }


        // Navigation properties
        public UserModel? Author { get; set; }
        public ICollection<ChapterModel> Chapters { get; set; }
        public ICollection<CommentModel> Comments { get; set; }
        public ICollection<FollowStoryModel> Followers { get; set; }
        public ICollection<StoryGenreModel> Genres { get; set; }

        public enum StoryStatus
        {
            Active,
            Inactive,
            Completed
        }
    }
}
