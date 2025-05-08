using PBL3.ViewModels.UserProfile;
using PBL3.Models;
using PBL3.Data;
using Microsoft.EntityFrameworkCore;

namespace PBL3.Service
{
    public interface IUserService
    {
        Task<UserProfileViewModel> GetUserProfile(int userId);
        Task<List<UserStoryCardViewModel>> GetUserStoryCard(int userId);
    }
}
