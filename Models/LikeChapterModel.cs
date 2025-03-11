using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class LikeChapterModel
    {

        [ForeignKey("User")]
        public int UserID { get; set; }
        [ForeignKey("Chapter")]
        public int ChapterID { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;

        //Navigation properties
        public ChapterModel? Chapter { get; set; }
        public UserModel? User { get; set; }
    }
}
