using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.ViewModels.Moderator;
using PBL3.Models;

namespace PBL3.Service.Story
{
    public class StoryQueryService : IStoryQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public StoryQueryService(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }
        public async Task<M_StoryViewModel> GetStoryViewModelAsync(int storyId)
        {
            var story = await _context.Stories.Include(s => s.Author)
                .FirstOrDefaultAsync(s => s.StoryID == storyId);
            if (story == null) return null;
            var storyVm = new M_StoryViewModel
            {
                StoryID = story.StoryID,
                Title = story.Title,
                CoverImage = story.CoverImage,
                Description = story.Description,
                Status = story.Status,
                CreatedAt = story.CreatedAt,
                UpdatedAt = story.UpdatedAt,
                isReported = await IsStoryReportedAsync(storyId)
            };
            if (!string.IsNullOrEmpty(storyVm.CoverImage))
            {
                storyVm.CoverImage = await _blobService.GetSafeImageUrlAsync(storyVm.CoverImage);
            }
            return storyVm;
        }
        public async Task<List<M_ChapterViewModel>> GetChaptersForStoryAsync(int storyId)
        {
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyId)
                .OrderBy(c => c.ChapterOrder)
                .Select(c => new M_ChapterViewModel
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    ChapterOrder = c.ChapterOrder,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Status = c.Status,
                    ViewCount = c.ViewCount,
                    isReported = _context.Notifications.Any(n => n.ChapterID == c.ChapterID && (n.Type == NotificationModel.NotificationType.ReportChapter) && n.IsRead == false)
                })
                .ToListAsync();
            return chapters;
        }
        public async Task<bool> IsStoryReportedAsync(int storyId)
        {
            bool isReported = await _context.Notifications.AnyAsync(n => n.StoryID == storyId && n.IsRead == false && n.Type == NotificationModel.NotificationType.ReportStory);
            bool chaptersReported = await _context.Notifications.AnyAsync(n => n.StoryID == storyId && n.IsRead == false && n.Type == NotificationModel.NotificationType.ReportChapter);
            return isReported || chaptersReported;
        }
    }
}
