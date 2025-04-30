using System.ComponentModel.DataAnnotations;

namespace PBL3.ViewModels.Chapter
{
    public class ChapterEditViewModel
    {
        public int ChapterID { get; set; }
        public int StoryID { get; set; }

        [StringLength(120, ErrorMessage = "Tên chương quá dài")]
        public string? Title { get; set; }
        public string? Content { get; set; }    
    }
}
