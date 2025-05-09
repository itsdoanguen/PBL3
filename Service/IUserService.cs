using PBL3.ViewModels.UserProfile;

namespace PBL3.Service
{
    public interface IUserService
    {
        Task<UserProfileViewModel> GetUserProfile(int userId);
        Task<List<UserStoryCardViewModel>> GetUserStoryCard(int userId);
    }
}
