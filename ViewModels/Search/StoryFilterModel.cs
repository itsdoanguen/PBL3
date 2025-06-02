namespace PBL3.ViewModels.Search
{
    public class StoryFilterModel
    {
        public string? TenTruyen { get; set; }
        public int? GenreId { get; set; }
        public List<string>? GenreNames { get; set; } // Cho phép chọn nhiều thể loại
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string? Status { get; set; }
        public string? AuthorName { get; set; }
    }
}
