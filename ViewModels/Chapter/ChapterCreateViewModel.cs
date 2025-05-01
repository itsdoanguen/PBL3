using System.ComponentModel.DataAnnotations;

namespace PBL3.ViewModels.Chapter
{
    public class ChapterCreateViewModel
    {
        [Required]
        public int StoryID { get; set; }

        [Required, StringLength(120)]
        public string Title { get; set; }
    }
}
