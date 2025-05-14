using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace PBL3.ViewModels.User
{
    public class IntroViewModel
    {
        // Header message displayed at the top of the page
        public string HeaderMessage { get; set; }

        // List of hot stories to display in the first section
        public List<StoryViewModel> HotStories { get; set; }

        // List of new stories to display in the second section
        public List<StoryViewModel> NewStories { get; set; }

        // List of completed stories to display in the third section
        public List<StoryViewModel> CompletedStories { get; set; }

        // All categories for the sidebar
        public List<CategoryViewModel> AllCategories { get; set; }

        // SelectList for category dropdown
        public SelectList CategorySelectList { get; set; }
    }

    // View model for story items
    public class StoryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }
        public bool IsCompleted { get; set; }
        public int ChapterCount { get; set; }
        public List<CategoryViewModel> Categories { get; set; }
        public ChapterViewModel LatestChapter { get; set; }
    }

    // View model for category items
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    // View model for chapter items
    public class ChapterViewModel
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; }
    }
} 