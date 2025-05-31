namespace PBL3.ViewModels.Search
{
    public class StoryFilterModel
    {
        public string? TenTruyen { get; set; }
        public int? GenreId { get; set; }
        public string? GenreName { get; set; }
        public List<string>? Genres { get; set; } 
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? Status { get; set; }
        public string? AuthorName { get; set; }
    }
}
