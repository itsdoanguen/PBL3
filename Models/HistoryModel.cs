using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class HistoryModel
    {
        [Key]
        public int HistoryID { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        [ForeignKey("Story")]
        public int StoryID { get; set; }

        [ForeignKey("Chapter")]
        public int? ChapterID { get; set; }

        public DateTime LastReadAt { get; set; } = DateTime.Now;

        // Navigation properties
        public UserModel? User { get; set; }
        public StoryModel? Story { get; set; }
        public ChapterModel? Chapter { get; set; }
    }
}
