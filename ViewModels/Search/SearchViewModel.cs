using PBL3.Models;

namespace PBL3.ViewModels.Search
{
    public class SearchViewModel
    {
        public int StoryID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
    }
}
