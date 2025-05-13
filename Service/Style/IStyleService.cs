using PBL3.Models;
using PBL3.ViewModels.Chapter;

namespace PBL3.Service.Style
{
    public interface IStyleService
    {
        Task<StyleModel> InitStyleForUserAsync(int userId);
        Task<StyleViewModel> GetStyleByUserIdAsync(int userId);
        Task<StyleModel> UpdateStyleAsync(StyleViewModel model);
    }
}
