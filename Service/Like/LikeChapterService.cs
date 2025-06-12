using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.LikeChapter;

namespace PBL3.Service.Like
{
    public class LikeChapterService : ILikeChapterService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public LikeChapterService(ApplicationDbContext context, BlobService blobService)
        {
            _blobService = blobService;
            _context = context;
        }

        public async Task<bool> LikeChapterAsync(int chapterId, int userId)
        {
            var existingLike = await _context.LikeChapters
                .FirstOrDefaultAsync(l => l.UserID == userId && l.ChapterID == chapterId);

            if (existingLike != null)
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
        public async Task<LikedChapterViewModel> GetLikedChaptersByUserAsync(int userId)
        {
            var liked = await (from l in _context.LikeChapters
                               join c in _context.Chapters on l.ChapterID equals c.ChapterID
                               join s in _context.Stories on c.StoryID equals s.StoryID
                               where l.UserID == userId
                               select new LikedChapterItemViewModel
                               {
                                   ChapterID = c.ChapterID,
                                   ChapterTitle = c.Title,
                                   StoryID = s.StoryID,
                                   StoryTitle = s.Title,
                                   StoryCoverImageUrl = s.CoverImage
                               }).ToListAsync();

            foreach (var item in liked)
            {
                item.StoryCoverImageUrl = await _blobService.GetSafeImageUrlAsync(item.StoryCoverImageUrl ?? string.Empty);
            }

            return new LikedChapterViewModel { LikedChapters = liked };
        }
    }
}
