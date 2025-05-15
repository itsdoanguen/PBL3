using PBL3.Models;
using PBL3.ViewModels;
using PBL3.Data;
using Microsoft.EntityFrameworkCore;

namespace PBL3.Service.Like
{
    public class LikeChapterService : ILikeChapterService
    {
        private readonly ApplicationDbContext _context;
        public LikeChapterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> LikeChapterAsync(int chapterId, int userId)
        {
            var existingLike = await _context.LikeChapters
                .FirstOrDefaultAsync(l => l.UserID == userId && l.ChapterID == chapterId);
            
            if(existingLike != null)
            {
                _context.LikeChapters.Remove(existingLike);
                await _context.SaveChangesAsync();

                return false;
            }

            var newLike = new LikeChapterModel
            {
                ChapterID = chapterId,
                UserID = userId,
                DateTime = DateTime.Now
            };
            _context.LikeChapters.Add(newLike);
            await _context.SaveChangesAsync();

            return false;
        }
        public async Task<bool> IsLikedByCurrentUserAsync(int chapterId, int userId)
        {
            var like = await _context.LikeChapters
                .FirstOrDefaultAsync(l => l.UserID == userId && l.ChapterID == chapterId);
            return like != null;
        }
        public async Task<int> GetLikeCountAsync(int chapterId)
        {
            return await _context.LikeChapters
                .CountAsync(l => l.ChapterID == chapterId);
        }
    }
}
