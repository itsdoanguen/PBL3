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

        // List of most liked stories to display in the liked section
        public List<StoryViewModel> MostLikedStories { get; set; }

        // List of new stories to display in the second section
        public List<StoryViewModel> NewStories { get; set; }

        // List of completed stories to display in the third section
        public List<StoryViewModel> CompletedStories { get; set; }

        // All categories for the sidebar
        public List<CategoryViewModel> AllCategories { get; set; }

        // SelectList for category dropdown
        public SelectList CategorySelectList { get; set; }

        // Top stories of the week
        public List<StoryViewModel> TopStoriesOfWeek { get; set; }

        // Followed stories (for authenticated users)
        public List<StoryViewModel> FollowedStories { get; set; }

        // Followed authors (for authenticated users)
        public List<AuthorViewModel> FollowedAuthors { get; set; }

        // Flag to indicate if user is authenticated
        public bool IsAuthenticated { get; set; }
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
        public DateTime? LastUpdated { get; set; }
        public string AuthorName { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
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

    // View model for author items
    public class AuthorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public int TotalStories { get; set; }
        public int TotalFollowers { get; set; }
        public bool IsFollowed { get; set; }
        public List<StoryViewModel> RecentStories { get; set; }
    }
}