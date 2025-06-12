using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.History;

namespace PBL3.Service.History
{
    public class HistoryService : IHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;

        public HistoryService(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task UpdateHistoryAsync(int userId, int storyId, int chapterId)
        {
            var existingHistory = await _context.Set<HistoryModel>()
                .FirstOrDefaultAsync(h => h.UserID == userId && h.StoryID == storyId);

            if (existingHistory != null)
            {
                existingHistory.ChapterID = chapterId;
                existingHistory.LastReadAt = DateTime.Now;
                _context.Update(existingHistory);
            }
            else
            {
                var newHistory = new HistoryModel
                {
                    UserID = userId,
                    StoryID = storyId,
                    ChapterID = chapterId,
                    LastReadAt = DateTime.Now
                };
                await _context.Set<HistoryModel>().AddAsync(newHistory);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<HistoryItemViewModel>> GetUserHistoryAsync(int userId)
        {
            var histories = await _context.Set<HistoryModel>()
                .Include(h => h.Story)
                    .ThenInclude(s => s.Author)
                .Include(h => h.Chapter)
                .Include(h => h.Story.Genres)
                    .ThenInclude(sg => sg.Genre)
                .Where(h => h.UserID == userId)
                .OrderByDescending(h => h.LastReadAt)
                .Select(h => new HistoryItemViewModel
                {
                    HistoryID = h.HistoryID,
                    StoryID = h.StoryID,
                    StoryTitle = h.Story.Title ?? "Không xác định",
                    StoryCover = h.Story.CoverImage ?? "/images/default-cover.jpg",
                    ChapterID = h.ChapterID ?? 0,
                    ChapterTitle = h.Chapter.Title ?? "",
                    ChapterOrder = h.Chapter.ChapterOrder,
                    ChapterLabel = $"Chương {h.Chapter.ChapterOrder}: {h.Chapter.Title}",
                    UpdatedAt = h.LastReadAt
                })
                .ToListAsync();

            foreach (var history in histories)
            {
                history.StoryCover = await _blobService.GetSafeImageUrlAsync(history.StoryCover);
            }

            return histories;
        }

        public async Task DeleteHistoryAsync(int userId, int? storyId = null)
        {
            var query = _context.Set<HistoryModel>().Where(h => h.UserID == userId);

            if (storyId.HasValue)
            {
                query = query.Where(h => h.StoryID == storyId.Value);
            }

            var histories = await query.ToListAsync();
            _context.Set<HistoryModel>().RemoveRange(histories);
            await _context.SaveChangesAsync();
        }
    }
}