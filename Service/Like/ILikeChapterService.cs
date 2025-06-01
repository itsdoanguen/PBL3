using PBL3.ViewModels.LikeChapter;

namespace PBL3.Service.Like
{
    public interface ILikeChapterService
    {
        Task<bool> LikeChapterAsync(int chapterId, int userId);
        Task<bool> IsLikedByCurrentUserAsync(int chapterId, int userId);
        Task<int> GetLikeCountAsync(int chapterId);
        Task<LikedChapterViewModel> GetLikedChaptersByUserAsync(int userId);
    }
}
