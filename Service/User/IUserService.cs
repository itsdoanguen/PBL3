using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.User
{
    public interface IUserService
    {
        Task<UserProfileViewModel> GetUserProfile(int userId);
        Task<List<UserStoryCardViewModel>> GetUserStoryCard(int userId);
        Task<(bool isSuccess, string errorMessage, UserProfileViewModel? updatedProfile)> UpdateUserProfileAsync(int userId, UserProfileViewModel profile, IFormFile? avatarUpload, IFormFile? bannerUpload);
        Task ToggleUpdateUserRoleAsync(int userId);
        Task<(bool isSuccess, string errorMessage)> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<(bool isSuccess, string errorMessage)> ForgotPasswordAsync(string email, Func<string, string, string?, Task> sendEmailFunc);
    }
}
