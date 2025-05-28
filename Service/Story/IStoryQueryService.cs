using System.Threading.Tasks;
using System.Collections.Generic;
using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;
using PBL3.ViewModels.Story;

namespace PBL3.Service.Story
{
    public interface IStoryQueryService
    {
        Task<M_StoryViewModel> GetStoryViewModelAsync(int storyId);
        Task<List<M_ChapterViewModel>> GetChaptersForStoryAsync(int storyId);
        Task<bool> IsStoryReportedAsync(int storyId);
        Task<StoryEditViewModel> GetStoryDetailForEditAsync(int storyID, int currentAuthorID);
        Task<StoryDetailViewModel> GetStoryDetailAsync(int storyID, int currentUserID);
        Task<List<GerneVM>> GetGerneForStoryAsync(int storyID);
        Task<List<ChapterInfo>> GetChapterForStoryAsync(int storyID);
        Task<int> GetTotalStoryWordAsync(int storyID);
        Task<double> RatingStoryAsync(int storyID);
    }
}
