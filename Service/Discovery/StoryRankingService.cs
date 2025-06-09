using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Service.Chapter;
using PBL3.Service.History;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.Discovery
{
    public class StoryRankingService : IStoryRankingService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IHistoryService _historyService;
        private readonly IChapterService _chapterService;
        public StoryRankingService(ApplicationDbContext context, BlobService blobService, IHistoryService historyService, IChapterService chapterService)
        {
            _context = context;
            _blobService = blobService;
            _historyService = historyService;
            _chapterService = chapterService;
        }

        private async Task<UserStoryCardViewModel> ToUserStoryCardViewModel(Models.StoryModel s)
        {
            var coverUrl = await _blobService.GetSafeImageUrlAsync(s.CoverImage ?? string.Empty);
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == s.StoryID)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            var totalWords = 0;
            foreach (var chapter in chapters)
            {
                totalWords += _chapterService.CountWordsInChapter(chapter.Content);
            }
            
            // Get author information
            var author = await _context.Users
                .Where(u => u.UserID == s.AuthorID)
                .Select(u => u.DisplayName ?? u.Email)
                .FirstOrDefaultAsync();

            return new UserStoryCardViewModel
            {
                StoryID = s.StoryID,
                Title = s.Title,
                Cover = coverUrl,
                TotalChapters = s.Chapters.Count,
                LastUpdated = s.UpdatedAt,
                Status = s.Status,
                Discription = s.Description,
                TotalViews = await _context.Chapters.Where(c => c.StoryID == s.StoryID).SumAsync(c => (int?)c.ViewCount) ?? 0,
                TotalLikes = await _context.LikeChapters.Where(l => l.Chapter.StoryID == s.StoryID).CountAsync(),
                TotalFollowers = await _context.FollowStories.CountAsync(f => f.StoryID == s.StoryID),
                TotalWords = totalWords,
                AuthorName = author ?? "Unknown Author",
                CreatedAt = s.CreatedAt
            };
        }

        public async Task<List<UserStoryCardViewModel>> GetTopStoriesOfWeekAsync(int topCount = 10)
        {
            DateTime oneWeekAgo = DateTime.Now.AddDays(-7);
            var stories = await _context.Stories
                .Where(s => s.Chapters.Any(c => c.CreatedAt >= oneWeekAgo)
                    && (s.Status == Models.StoryModel.StoryStatus.Active
                        || s.Status == Models.StoryModel.StoryStatus.Completed))
                .Select(s => new
                {
                    Story = s,
                    WeeklyViews = _context.Chapters
                        .Where(c => c.StoryID == s.StoryID && c.CreatedAt >= oneWeekAgo)
                        .Sum(c => (int?)c.ViewCount) ?? 0
                })
                .OrderByDescending(s => s.WeeklyViews)
                .Take(topCount)
                .Select(s => s.Story)
                .ToListAsync();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in stories)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetRecommendedStoryAsync(int userId)
        {
            var userHistory = await _historyService.GetUserHistoryAsync(userId);
            List<Models.StoryModel> stories;
            if (userHistory == null || !userHistory.Any())
            {
                stories = await _context.Stories
                    .Where(s => s.Status == Models.StoryModel.StoryStatus.Active
                        || s.Status == Models.StoryModel.StoryStatus.Completed)
                    .Include(s => s.Followers)
                    .Include(s => s.Chapters)
                    .ThenInclude(c => c.Likes)
                    .Include(s => s.Genres)
                    .OrderByDescending(s => s.UpdatedAt)
                    .ToListAsync();
            }
            else
            {
                var viewedStoryIds = userHistory.Select(h => h.StoryID).ToList();
                var userLikedGenresId = await _context.StoryGenres
                    .Where(sg => viewedStoryIds.Contains(sg.StoryID))
                    .Select(sg => sg.GenreID)
                    .Distinct()
                    .ToListAsync();
                stories = await _context.Stories
                    .Where(s => s.Status == Models.StoryModel.StoryStatus.Active
                        || s.Status == Models.StoryModel.StoryStatus.Completed)
                    .Where(s => s.Genres.Any(g => userLikedGenresId.Contains(g.GenreID))
                                && !viewedStoryIds.Contains(s.StoryID))
                    .Include(s => s.Followers)
                    .Include(s => s.Chapters)
                    .ThenInclude(c => c.Likes)
                    .Include(s => s.Genres)
                    .OrderByDescending(s => s.UpdatedAt)
                    .ToListAsync();
            }
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in stories)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetStoriesByViewAsync()
        {
            var stories = await _context.Stories
                .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                .Select(s => new
                {
                    Story = s,
                    TotalViews = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0
                })
                .OrderByDescending(s => s.TotalViews)
                .Select(s => s.Story)
                .ToListAsync();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in stories)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetStoriesByFollowAsync()
        {
            var stories = await _context.Stories
                .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                .Select(s => new
                {
                    Story = s,
                    TotalFollowers = _context.FollowStories.Count(f => f.StoryID == s.StoryID)
                })
                .OrderByDescending(s => s.TotalFollowers)
                .Select(s => s.Story)
                .ToListAsync();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in stories)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetStoriesByWordCountAsync()
        {
            var stories = await _context.Stories
                .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                .Include(s => s.Followers)
                .Include(s => s.Chapters)
                .ThenInclude(c => c.Likes)
                .ToListAsync();
            var storyWordCounts = new List<(Models.StoryModel story, int wordCount)>();
            foreach (var s in stories)
            {
                var chapters = s.Chapters;
                int totalWords = 0;
                foreach (var chapter in chapters)
                {
                    totalWords += _chapterService.CountWordsInChapter(chapter.Content);
                }
                storyWordCounts.Add((s, totalWords));
            }
            var sorted = storyWordCounts.OrderByDescending(x => x.wordCount).Select(x => x.story).ToList();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in sorted)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetStoriesByLikeAsync()
        {
            var stories = await _context.Stories
                .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                .Include(s => s.Chapters)
                .ThenInclude(c => c.Likes)
                .Include(s => s.Followers)
                .ToListAsync();
            var sorted = stories.OrderByDescending(s => s.Chapters.Sum(c => c.Likes != null ? c.Likes.Count : 0)).ToList();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in sorted)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetStoriesByUpdatedAsync()
        {
            var stories = await _context.Stories
                .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                .Include(s => s.Followers)
                .Include(s => s.Chapters)
                .ThenInclude(c => c.Likes)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in stories)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetCompletedStoriesAsync()
        {
            var stories = await _context.Stories
                .Where(s => s.Status == Models.StoryModel.StoryStatus.Completed)
                .Include(s => s.Followers)
                .Include(s => s.Chapters)
                .ThenInclude(c => c.Likes)
                .OrderByDescending(s => s.UpdatedAt)
                .ToListAsync();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in stories)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }
    }
}
