using PBL3.ViewModels.User;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.User
{
    public interface IUserService
    {
        Task<UserProfileViewModel> GetUserProfile(int userId);
        Task<List<UserStoryCardViewModel>> GetUserStoryCard(int userId);

        Task<UserIndexViewModel> GetUserIndexViewModelAsync(int userId);
    }
}
