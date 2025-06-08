using PBL3.ViewModels.User;

namespace PBL3.Service.Dashboard
{
    public interface IDashboardService
    {
        Task<List<StoryViewModel>> GetTopStoriesOfWeekAsync(int count = 5);
        Task<List<StoryViewModel>> GetHotStoriesAsync(int count = 5);
        Task<List<StoryViewModel>> GetHotStoriesByCategoryAsync(int categoryId, int count = 5);
        Task<List<StoryViewModel>> GetMostLikedStoriesAsync(int count = 5);
        Task<List<StoryViewModel>> GetNewStoriesAsync(int count = 5);
        Task<List<StoryViewModel>> GetCompletedStoriesAsync(int count = 5);
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<List<StoryViewModel>> GetTopFollowedStoriesAsync(int count = 20);
        Task<List<AuthorViewModel>> GetFollowedAuthorsAsync(int userId, int count = 20);
        Task<IntroViewModel> GetDashboardDataAsync(int? userId = null);
    }
}