using System.Collections.Generic;

namespace PBL3.ViewModels.Admin
{
    public class AdminManageSystemViewModel
    {
        public List<UserInfo> Users { get; set; }
        public List<StoryInfo> ActiveStories { get; set; }
        public List<StoryInfo> CompletedStories { get; set; }
        public List<GenreInfo> Genres { get; set; }
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int TotalWarnings { get; set; }
        public bool IsActive { get; set; }
    }

    public class StoryInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int ChapterCount { get; set; }
        public string Status { get; set; }
    }

    public class GenreInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StoryCount { get; set; }
    }
}
