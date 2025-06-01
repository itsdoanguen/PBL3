using PBL3.ViewModels.FollowStory;
using PBL3.ViewModels.FollowUser;

namespace PBL3.Service.Follow
{
    public interface IFollowService
    {
        // Story follow
        Task<(bool isSuccess, string Message)> FollowStoryAsync(int userId, int storyId);
        Task<(bool isSuccess, string Message)> UnfollowStoryAsync(int userId, int storyId);
        Task<bool> IsFollowingStoryAsync(int userId, int storyId);
        Task<int> CountStoryFollowersAsync(int storyId);
        Task<FollowStoryViewModel> GetFollowStoryList(int userId);

        // User follow
        Task<(bool isSuccess, string Message)> FollowUserAsync(int followerId, int followingId);
        Task<(bool isSuccess, string Message)> UnfollowUserAsync(int followerId, int followingId);
        Task<bool> IsFollowingUserAsync(int followerId, int followingId);
        Task<int> CountUserFollowersAsync(int userId);
        Task<int> CountUserFollowingsAsync(int userId);
        Task<List<UserFollowItemViewModel>> GetFollowingUsersAsync(int userId);
        Task<List<UserFollowItemViewModel>> GetFollowerUsersAsync(int userId);
    }
}
