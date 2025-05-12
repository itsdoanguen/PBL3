using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PBL3.ViewModels.Story
{
    public class StoryCreateViewModel
    {
        [Required(ErrorMessage = "Tiêu đề truyện là bắt buộc"), StringLength(255)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả truyện là bắt buộc")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Thể loại")]
        public IFormFile? UploadCover { get; set; }

        [Display(Name = "Thể loại")]
        [Required(ErrorMessage = "Thể loại là bắt buộc, hãy chọn ít nhất 1")]
        public List<int> GenreIDs { get; set; } = new List<int>();

        public List<GerneVM> availbleGenres { get; set; } = new List<GerneVM>();
    }

    public class GerneVM
    {
        public int GenreID { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}