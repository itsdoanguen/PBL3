using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class StoryGenreModel
    {
        [ForeignKey("Story")]
        public int StoryID { get; set; }
        [ForeignKey("Genre")]
        public int GenreID { get; set; }

        // Navigation properties
        public StoryModel? Story { get; set; }
        public GenreModel? Genre { get; set; }
    }
}
