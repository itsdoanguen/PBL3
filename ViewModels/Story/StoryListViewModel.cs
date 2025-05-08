using System;
using System.Collections.Generic;

namespace PBL3.Models.ViewModels
{
    public class StoryListItemViewModel
    {
        public int StoryID { get; set; }
        public string Title { get; set; }
        public string StoryDetailUrl { get; set; }
        public string AuthorName { get; set; }
        public List<string> GenreNames { get; set; }
        public string LastChapterName { get; set; } // Tên chương mới nhất
        public DateTime? LastUpdatedAt { get; set; } // Thời gian cập nhật chương mới nhất hoặc truyện

        public StoryListItemViewModel()
        {
            GenreNames = new List<string>();
        }
    }
}
