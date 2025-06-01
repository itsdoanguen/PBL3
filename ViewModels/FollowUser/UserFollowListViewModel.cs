using System.Collections.Generic;

namespace PBL3.ViewModels.FollowUser
{
    public class UserFollowItemViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? ShortBio { get; set; }
    }

    public class UserFollowListViewModel
    {
        public List<UserFollowItemViewModel> FollowingUsers { get; set; } = new();
        public List<UserFollowItemViewModel> FollowerUsers { get; set; } = new();
    }
}
