using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.Moderator
{
    public interface IModeratorService
    {
        Task<ViewUserViewModel> GetViewUserViewModelAsync(int userId);
        Task<ViewStoryViewModel> GetViewStoryViewModelAsync(int storyId);
        Task<List<ViewModels.Moderator.UserProfileViewModel>> GetListUserForModeratorAsync();
        Task<List<UserStoryCardViewModel>> GetListStoriesForModeratorAsync();
    }
}
