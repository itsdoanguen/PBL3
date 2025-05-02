using System.Threading.Tasks;
using PBL3.Models;
using PBL3.ViewModels.Chapter;

namespace PBL3.Service
{
    public interface IChapterService
    {
        Task<ChapterDetailViewModel> GetChapterDetailAsync(int chapterId, string currentUserId, Func<string, bool> checkCookieExists, Action<string, string, CookieOptions> setCookieAsync);
        Task DeleteChapterAsync(int chapterId, int storyId);
        Task <ChapterModel> CreateChapterAsync(ChapterCreateViewModel chapterViewModel);
        Task<ChapterEditViewModel> GetChapterForEditAsync(int chapterId, int storyId, int currentUserId);
        Task<bool> UpdateChapterAsync(ChapterEditViewModel model);
        Task<(bool Success, string Message, int StoryId)> UpdateChapterStatusAsync(int chapterId, int currentUserId, string newStatus);
        Task<List<ChapterSummaryViewModel>> GetChaptersForStoryAsync(int storyId);
    }
}
