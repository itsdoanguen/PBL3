using System.Threading.Tasks;
using System.Collections.Generic;
using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.User
{
    public interface IUserQueryService
    {
        Task<ViewModels.Moderator.UserProfileViewModel> GetUserProfileAsync(int userId);
        Task<List<UserStoryCardViewModel>> GetUserStoriesAsync(int userId);
        Task<List<CommentViewModel>> GetUserCommentsAsync(int userId);
    }
}
