using PBL3.ViewModels.User;

namespace PBL3.Service.Dashboard
{
    public interface IDashboardService
    {
        Task<List<StoryViewModel>> GetTopStoriesOfWeekAsync(int count = 10);
        Task<List<StoryViewModel>> GetHotStoriesAsync(int count = 16);
        Task<List<StoryViewModel>> GetHotStoriesByCategoryAsync(int categoryId, int count = 16);
        Task<List<StoryViewModel>> GetNewStoriesAsync(int count = 15);
        Task<List<StoryViewModel>> GetCompletedStoriesAsync(int count = 15);
        Task<List<CategoryViewModel>> GetAllCategoriesAsync();
        Task<List<StoryViewModel>> GetFollowedStoriesAsync(int userId, int count = 10);
        Task<List<AuthorViewModel>> GetFollowedAuthorsAsync(int userId, int count = 10);
        Task<IntroViewModel> GetDashboardDataAsync(int? userId = null);
    }
}