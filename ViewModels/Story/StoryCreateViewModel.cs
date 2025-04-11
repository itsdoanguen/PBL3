using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PBL3.ViewModels.Story
{
    public class StoryCreateViewModel
    {
        [Required, StringLength(255)]
        public string Title { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        public IFormFile? UploadCover { get; set; } 
    }
}
