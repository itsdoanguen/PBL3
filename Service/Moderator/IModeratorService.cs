using PBL3.ViewModels.Moderator;

namespace PBL3.Service.Moderator
{
    public interface IModeratorService
    {
        Task<ViewUserViewModel> GetViewUserViewModelAsync(int userId);
    }
}
