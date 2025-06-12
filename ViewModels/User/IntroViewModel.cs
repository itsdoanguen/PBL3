using Microsoft.AspNetCore.Mvc.Rendering;

namespace PBL3.ViewModels.User
{
    /// <summary>
    /// ViewModel hoàn chỉnh cho trang chủ - hỗ trợ cả trải nghiệm khách và người dùng đã đăng nhập
    /// </summary>
    public class IntroViewModel
    {
        // ===== BASIC INFORMATION =====
        /// <summary>
        /// Header message displayed at the top of the page
        /// </summary>
        public string HeaderMessage { get; set; } = "CHÀO MỪNG BẠN ĐẾN VỚI THẾ GIỚI TRUYỆN CỦA CHÚNG TÔI!";

        /// <summary>
        /// Flag to indicate if user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// User profile summary (for authenticated users only)
        /// </summary>
        public UserProfileSummary? UserProfile { get; set; }

        // ===== STORY COLLECTIONS =====
        /// <summary>
        /// List of hot stories to display in the first section
        /// </summary>
        public List<StoryViewModel> HotStories { get; set; } = new List<StoryViewModel>();

        /// <summary>
        /// List of most liked stories to display in the liked section
        /// </summary>
        public List<StoryViewModel> MostLikedStories { get; set; } = new List<StoryViewModel>();

        /// <summary>
        /// List of new stories to display in the second section
        /// </summary>
        public List<StoryViewModel> NewStories { get; set; } = new List<StoryViewModel>();

        /// <summary>
        /// List of completed stories to display in the third section
        /// </summary>
        public List<StoryViewModel> CompletedStories { get; set; } = new List<StoryViewModel>();

        /// <summary>
        /// Top stories of the week
        /// </summary>
        public List<StoryViewModel> TopStoriesOfWeek { get; set; } = new List<StoryViewModel>();

        /// <summary>
        /// Top followed stories (stories with most followers)
        /// </summary>
        public List<StoryViewModel> TopFollowedStories { get; set; } = new List<StoryViewModel>();

        // ===== PERSONALIZED CONTENT (FOR AUTHENTICATED USERS) =====
        /// <summary>
        /// Stories from authors the user follows
        /// </summary>
        public List<StoryViewModel> FollowingStories { get; set; } = new List<StoryViewModel>();

        /// <summary>
        /// Personalized story recommendations based on user reading history
        /// </summary>
        public List<StoryViewModel> RecommendedStories { get; set; } = new List<StoryViewModel>();

        /// <summary>
        /// User's recent reading history
        /// </summary>
        public List<RecentReadViewModel> RecentlyRead { get; set; } = new List<RecentReadViewModel>();

        /// <summary>
        /// User activity statistics
        /// </summary>
        public UserActivityStats? ActivityStats { get; set; }

        // ===== AUTHORS AND CATEGORIES =====
        /// <summary>
        /// Followed authors (for authenticated users)
        /// </summary>
        public List<AuthorViewModel> FollowedAuthors { get; set; } = new List<AuthorViewModel>();

        /// <summary>
        /// All categories for the sidebar
        /// </summary>
        public List<CategoryViewModel> AllCategories { get; set; } = new List<CategoryViewModel>();

        /// <summary>
        /// SelectList for category dropdown
        /// </summary>
        public SelectList CategorySelectList { get; set; }

        /// <summary>
        /// User's favorite categories (for authenticated users)
        /// </summary>
        public List<CategorySummary> FavoriteCategories { get; set; } = new List<CategorySummary>();

        // ===== REAL STATISTICS =====
        /// <summary>
        /// Real database statistics for hero section
        /// </summary>
        public DatabaseStats Statistics { get; set; } = new DatabaseStats();
    }

    // ===== STORY VIEWMODEL =====
    /// <summary>
    /// View model for story items
    /// </summary>
    public class StoryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = "/image/default-cover.jpg";
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }
        public bool IsCompleted { get; set; }
        public int ChapterCount { get; set; }
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public ChapterViewModel? LatestChapter { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int FollowCount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ===== CATEGORY VIEWMODELS =====
    /// <summary>
    /// View model for category items
    /// </summary>
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extended category information with statistics
    /// </summary>
    public class CategorySummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int StoryCount { get; set; }
    }

    // ===== CHAPTER VIEWMODEL =====
    /// <summary>
    /// View model for chapter items
    /// </summary>
    public class ChapterViewModel
    {
        public int Id { get; set; }
        public int ChapterNumber { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    // ===== AUTHOR VIEWMODEL =====
    /// <summary>
    /// View model for author items
    /// </summary>
    public class AuthorViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = "/image/default-avatar.png";
        public int TotalStories { get; set; }
        public int TotalFollowers { get; set; }
        public bool IsFollowed { get; set; }
        public List<StoryViewModel> RecentStories { get; set; } = new List<StoryViewModel>();
    }

    // ===== USER-SPECIFIC VIEWMODELS =====
    /// <summary>
    /// User profile summary for authenticated users
    /// </summary>
    public class UserProfileSummary
    {
        public int UserID { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Avatar { get; set; } = "/image/default-avatar.png";
        public int TotalFollowing { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalStoriesRead { get; set; }
    }

    /// <summary>
    /// Recent reading history item
    /// </summary>
    public class RecentReadViewModel
    {
        public int StoryID { get; set; }
        public string StoryTitle { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public int ChapterID { get; set; }
        public string ChapterTitle { get; set; } = string.Empty;
        public DateTime LastRead { get; set; }
        public int ReadProgress { get; set; } // Percentage read
        public int TotalChapters { get; set; }
    }

    /// <summary>
    /// User activity statistics
    /// </summary>
    public class UserActivityStats
    {
        public int TotalStoriesRead { get; set; }
        public int TotalChaptersRead { get; set; }
        public int TotalCommentsPosted { get; set; }
        public int TotalLikesGiven { get; set; }
        public DateTime JoinedDate { get; set; }
        public int DaysActive { get; set; }
        public int CurrentStreak { get; set; } // Consecutive reading days
        public DateTime LastActiveDate { get; set; }
    }

    /// <summary>
    /// Real database statistics for the application
    /// </summary>
    public class DatabaseStats
    {
        public int TotalActiveStories { get; set; }
        public int TotalNewStories { get; set; } // Stories created in last 7 days
        public int TotalCompletedStories { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public int TotalAuthors { get; set; } // Users who have written stories
    }
}