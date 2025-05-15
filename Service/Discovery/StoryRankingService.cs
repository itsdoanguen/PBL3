using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.Discovery
{
    public class StoryRankingService : IStoryRankingService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public StoryRankingService(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<List<UserStoryCardViewModel>> GetTopStoriesOfWeekAsync(int topCount = 10)
        {
            DateTime oneWeekAgo = DateTime.Now.AddDays(-7);

            var stories = await _context.Stories
                .Where(s => s.Chapters.Any(c => c.CreatedAt >= oneWeekAgo) && s.Status != Models.StoryModel.StoryStatus.Inactive)
                .Select(s => new
                {
                    Story = s,
                    TotalViews = s.Chapters
                        .Where(c => c.CreatedAt >= oneWeekAgo)
                        .Sum(c => c.ViewCount)
                })
                .OrderByDescending(s => s.TotalViews)
                .Take(topCount)
                .Select(s => new UserStoryCardViewModel
                {
                    StoryID = s.Story.StoryID,
                    Title = s.Story.Title,
                    Cover = s.Story.CoverImage,
                    TotalChapters = s.Story.Chapters.Count,
                    LastUpdated = s.Story.Chapters
                        .Where(c => c.CreatedAt >= oneWeekAgo)
                        .OrderByDescending(c => c.UpdatedAt)
                        .Select(c => c.UpdatedAt)
                        .FirstOrDefault(),
                    Status = s.Story.Status
                })
                .ToListAsync();


            foreach (var story in stories)
            {
                story.Cover = await _blobService.GetSafeImageUrlAsync(story.Cover);
            }

            return stories;
        }
    }
}
