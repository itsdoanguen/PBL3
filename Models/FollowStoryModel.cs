using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class FollowStoryModel
    {
        [ForeignKey("User")]
        public int UserID { get; set; }
        [ForeignKey("Story")]
        public int StoryID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //Navigation properties
        public StoryModel? Story { get; set; }
        public UserModel? User { get; set; }
    }
}
