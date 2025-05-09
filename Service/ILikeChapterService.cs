namespace PBL3.Service
{
    public interface ILikeChapterService
    {
        Task<bool> LikeChapterAsync(int chapterId, int userId);
        Task<bool> IsLikedByCurrentUserAsync(int chapterId, int userId);
        Task<int> GetLikeCountAsync(int chapterId);
    }
}
