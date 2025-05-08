using System.ComponentModel.DataAnnotations;
using PBL3.Models;

namespace PBL3.ViewModels.Chapter
{
    public class ChapterEditViewModel
    {
        public int ChapterID { get; set; }
        public int StoryID { get; set; }

        [StringLength(120, ErrorMessage = "Tên chương quá dài")]
        public string? Title { get; set; }
        public string? Content { get; set; }    

        public ChapterStatus ChapterStatus { get; set; } = ChapterStatus.Inactive;
    }
}
