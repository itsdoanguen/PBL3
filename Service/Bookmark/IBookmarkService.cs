using PBL3.Models;
using PBL3.ViewModels.Bookmark;

namespace PBL3.Service.Bookmark
{
    public interface IBookmarkService
    {
        Task<(bool isSuccess, string Message)> ToggleBookmarkAsync(int userId, int chapterId);
        Task<bool> IsBookmarkedAsync(int userId, int chapterId);
        Task<int> CountBookmarksAsync(int chapterId);
        Task<List<BookmarkModel>> GetBookmarksByUserAsync(int userId);
        Task<List<BookmarkModel>> GetBookmarksByChapterAsync(int chapterId);
        Task<bool> RemoveBookmarkAsync(int userId, int chapterId);
        Task<BookmarkViewModel> GetBookmarkListAsync(int userID);
    }
}
