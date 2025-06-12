using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Bookmark;

namespace PBL3.Service.Bookmark
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public BookmarkService(ApplicationDbContext context, BlobService blobService)
        {
            _blobService = blobService;
            _context = context;
        }

        public async Task<(bool isSuccess, string Message)> ToggleBookmarkAsync(int userId, int chapterId)
        {
            var bookmark = await _context.Bookmarks.FirstOrDefaultAsync(b => b.UserID == userId && b.ChapterID == chapterId);
            if (bookmark != null)
            {
                _context.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();
                return (true, "Bookmark removed.");
            }
            else
            {
                var newBookmark = new BookmarkModel { UserID = userId, ChapterID = chapterId };
                _context.Bookmarks.Add(newBookmark);
                await _context.SaveChangesAsync();
                return (true, "Bookmarked.");
            }
        }

        public async Task<bool> IsBookmarkedAsync(int userId, int chapterId)
        {
            return await _context.Bookmarks.AnyAsync(b => b.UserID == userId && b.ChapterID == chapterId);
        }

        public async Task<int> CountBookmarksAsync(int chapterId)
        {
            return await _context.Bookmarks.CountAsync(b => b.ChapterID == chapterId);
        }

        public async Task<List<BookmarkModel>> GetBookmarksByUserAsync(int userId)
        {
            return await _context.Bookmarks.Where(b => b.UserID == userId).ToListAsync();
        }

        public async Task<List<BookmarkModel>> GetBookmarksByChapterAsync(int chapterId)
        {
            return await _context.Bookmarks.Where(b => b.ChapterID == chapterId).ToListAsync();
        }

        public async Task<bool> RemoveBookmarkAsync(int userId, int chapterId)
        {
            var bookmark = await _context.Bookmarks.FirstOrDefaultAsync(b => b.UserID == userId && b.ChapterID == chapterId);
            if (bookmark != null)
            {
                _context.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<BookmarkViewModel> GetBookmarkListAsync(int userId)
        {
            var items = await (from b in _context.Bookmarks
                               join c in _context.Chapters on b.ChapterID equals c.ChapterID
                               join s in _context.Stories on c.StoryID equals s.StoryID
                               where b.UserID == userId && (s.Status == StoryModel.StoryStatus.Active || s.Status == StoryModel.StoryStatus.Completed)
                               select new BookmarkItemViewModel
                               {
                                   StoryID = s.StoryID,
                                   StoryTitle = s.Title,
                                   StoryCoverImageUrl = s.CoverImage,
                                   ChapterID = c.ChapterID,
                                   ChapterTitle = c.Title

                               }).ToListAsync();

            foreach (var item in items)
            {
                item.StoryCoverImageUrl = await _blobService.GetSafeImageUrlAsync(item.StoryCoverImageUrl ?? string.Empty);
            }
            return new BookmarkViewModel
            {
                UserID = userId,
                BookmarkedStories = items
            };
        }

    }
}
