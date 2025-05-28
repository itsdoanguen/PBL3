using System.Threading.Tasks;
using System.Collections.Generic;
using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.Story
{
    public interface IStoryQueryService
    {
        Task<M_StoryViewModel> GetStoryViewModelAsync(int storyId);
        Task<List<M_ChapterViewModel>> GetChaptersForStoryAsync(int storyId);
        Task<bool> IsStoryReportedAsync(int storyId);
    }
}
