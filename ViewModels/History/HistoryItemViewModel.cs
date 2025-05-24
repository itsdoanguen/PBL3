using System;
using System.Collections.Generic;

namespace PBL3.ViewModels.History
{
    public class HistoryItemViewModel
    {
        public int HistoryID { get; set; }
        public int StoryID { get; set; }
        public string StoryTitle { get; set; }
        public string StoryCover { get; set; }
        public int ChapterID { get; set; }
        public string ChapterTitle { get; set; }
        public int ChapterOrder { get; set; }
        public string ChapterLabel { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserHistoryViewModel
    {
        public List<HistoryItemViewModel> Items { get; set; }
    }
}
