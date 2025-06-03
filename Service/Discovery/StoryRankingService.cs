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
            return new UserStoryCardViewModel
            {
                StoryID = s.StoryID,
                Title = s.Title,
                Cover = coverUrl,
                TotalChapters = s.Chapters.Count,
                LastUpdated = s.UpdatedAt,
                Status = s.Status,
                Discription = s.Description,
                TotalViews = s.Chapters.Sum(c => c.ViewCount),
                TotalLikes = s.Chapters.Sum(c => c.Likes != null ? c.Likes.Count : 0),
                TotalFollowers = s.Followers != null ? s.Followers.Count : 0,
                TotalWords = totalWords
            };
        }

        public async Task<List<UserStoryCardViewModel>> GetTopStoriesOfWeekAsync(int topCount = 10)
        {
            DateTime oneWeekAgo = DateTime.Now.AddDays(-7);
            var stories = await _context.Stories
                .Where(s => s.Chapters.Any(c => c.CreatedAt >= oneWeekAgo)
                    && (s.Status == Models.StoryModel.StoryStatus.Active
                        || s.Status == Models.StoryModel.StoryStatus.Completed))
                .OrderByDescending(s => s.Chapters.Where(c => c.CreatedAt >= oneWeekAgo).Sum(c => c.ViewCount))
                .Take(topCount)
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
                stories = await _context.Stories.Where(s => s.Status == Models.StoryModel.StoryStatus.Active
                    || s.Status == Models.StoryModel.StoryStatus.Completed)
                    .Where(s => s.Genres.Any(g => userLikedGenresId.Contains(g.GenreID))
                                && !viewedStoryIds.Contains(s.StoryID))
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
                .OrderByDescending(s => s.Chapters.Sum(c => c.ViewCount))
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
                .Include(s => s.Followers)
                .ToListAsync();
            var sorted = stories.OrderByDescending(s => s.Followers != null ? s.Followers.Count : 0).ToList();
            var result = new List<UserStoryCardViewModel>();
            foreach (var s in sorted)
            {
                result.Add(await ToUserStoryCardViewModel(s));
            }
            return result;
        }

        public async Task<List<UserStoryCardViewModel>> GetStoriesByWordCountAsync()
        {
            var stories = await _context.Stories
                .Where(s => s.Status == Models.StoryModel.StoryStatus.Active || s.Status == Models.StoryModel.StoryStatus.Completed)
                .ToListAsync();
            var storyWordCounts = new List<(Models.StoryModel story, int wordCount)>();
            foreach (var s in stories)
            {
                var chapters = await _context.Chapters.Where(c => c.StoryID == s.StoryID).ToListAsync();
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
    }
}
