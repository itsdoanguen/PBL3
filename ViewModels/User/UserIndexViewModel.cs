using PBL3.Models;
using PBL3.ViewModels.Story;
using PBL3.ViewModels.UserProfile;
using System.Collections.Generic;

namespace PBL3.ViewModels.User
{
   /* public class UserIndexViewModel
    {
        // Thông tin cơ bản của người dùng
        public UserProfileViewModel UserProfile { get; set; }
        
        // Danh sách truyện đang theo dõi
        public List<StoryCardViewModel> FollowingStories { get; set; } = new List<StoryCardViewModel>();
        
        // Danh sách truyện đã đọc gần đây
        public List<RecentReadViewModel> RecentlyRead { get; set; } = new List<RecentReadViewModel>();
        
        // Danh sách truyện đề xuất
        public List<StoryCardViewModel> RecommendedStories { get; set; } = new List<StoryCardViewModel>();
        
        // Thống kê hoạt động
        public UserActivityStats ActivityStats { get; set; }
    }
   */
    public class RecentReadViewModel
    {
        public int StoryID { get; set; }
        public string StoryTitle { get; set; }
        public string CoverImage { get; set; }
        public int ChapterID { get; set; }
        public string ChapterTitle { get; set; }
        public DateTime LastRead { get; set; }
        public int ReadProgress { get; set; } // Phần trăm đã đọc
    }

    public class UserActivityStats
    {
        public int TotalStoriesRead { get; set; }
        public int TotalChaptersRead { get; set; }
        public int TotalCommentsPosted { get; set; }
        public int TotalLikesGiven { get; set; }
        public DateTime JoinedDate { get; set; }
        public int DaysActive { get; set; }
    }
}