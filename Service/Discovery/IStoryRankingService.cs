using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.Discovery
{
    public interface IStoryRankingService
    {
        Task<List<UserStoryCardViewModel>> GetTopStoriesOfWeekAsync(int topCount = 10);
        Task<List<UserStoryCardViewModel>> GetRecommendedStoryAsync(int userId);
        Task<List<UserStoryCardViewModel>> GetStoriesByViewAsync();
        Task<List<UserStoryCardViewModel>> GetStoriesByFollowAsync();
        Task<List<UserStoryCardViewModel>> GetStoriesByWordCountAsync();
    }
}
