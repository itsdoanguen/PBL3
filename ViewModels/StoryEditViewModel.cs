using System.ComponentModel.DataAnnotations;
using PBL3.Models;

namespace PBL3.ViewModels
{
    public class StoryEditViewModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CoverImage { get; set; }
        public ICollection<ChapterModel>? Chapters { get; set; }
    }
}
