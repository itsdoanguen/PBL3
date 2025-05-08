namespace PBL3.Service
{
    public interface ILikeChapterService
    {
        Task<(bool Liked, int LikeCount)> LikeChapterAsync(int chapterId, int userId);
    }
}
