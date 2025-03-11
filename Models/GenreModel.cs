using System.ComponentModel.DataAnnotations;

namespace PBL3.Models
{
    public class GenreModel
    {
        [Key]
        public int GenreID { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; }

        //Navigation properties
        public ICollection<StoryGenreModel> StoryGenres { get; set; }
    }
}
