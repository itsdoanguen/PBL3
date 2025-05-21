using PBL3.Models;
using PBL3.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PBL3.Service.Bookmark
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _context;
        public BookmarkService(ApplicationDbContext context)
        {
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
    }
}
