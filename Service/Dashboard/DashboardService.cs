using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.User;
using static PBL3.Models.StoryModel;

namespace PBL3.Service.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;

        public DashboardService(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<List<StoryViewModel>> GetTopStoriesOfWeekAsync(int count = 12)
        {
            DateTime oneWeekAgo = DateTime.Now.AddDays(-7);

            var stories = await _context.Stories
                .Where(s => s.Chapters.Any(c => c.CreatedAt >= oneWeekAgo) && s.Status == StoryStatus.Active)
                .Select(s => new
                {
                    Story = s,
                    TotalViews = s.Chapters
                        .Where(c => c.CreatedAt >= oneWeekAgo)
                        .Sum(c => c.ViewCount)
                })
                .OrderByDescending(s => s.TotalViews)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.Story.StoryID,
                    Title = s.Story.Title,
                    CoverImageUrl = s.Story.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = true,
                    IsNew = s.Story.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Story.Status == StoryStatus.Completed,
                    ChapterCount = s.Story.Chapters.Count,
                    LastUpdated = s.Story.UpdatedAt,
                    AuthorName = s.Story.Author.DisplayName,
                    ViewCount = s.TotalViews,
                    LikeCount = s.Story.Chapters.SelectMany(c => c.Likes).Count()
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        public async Task<List<StoryViewModel>> GetHotStoriesAsync(int count = 12)
        {
            var stories = await _context.Stories
                .Where(s => s.Status == StoryStatus.Active)
                .OrderByDescending(s => s.Chapters.Sum(c => c.ViewCount))
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = true,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName,
                    ViewCount = s.Chapters.Sum(c => c.ViewCount),
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count()
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        public async Task<List<StoryViewModel>> GetHotStoriesByCategoryAsync(int categoryId, int count = 12)
        {
            if (categoryId == 0)
            {
                return await GetHotStoriesAsync(count);
            }

            var stories = await _context.Stories
                .Where(s => s.Status == StoryStatus.Active &&
                       s.Genres.Any(sg => sg.GenreID == categoryId))
                .OrderByDescending(s => s.Chapters.Sum(c => c.ViewCount))
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = true,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName,
                    ViewCount = s.Chapters.Sum(c => c.ViewCount),
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count()
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        public async Task<List<StoryViewModel>> GetMostLikedStoriesAsync(int count = 12)
        {
            var stories = await _context.Stories
                .Where(s => s.Status == StoryStatus.Active)
                .Select(s => new
                {
                    Story = s,
                    TotalLikes = s.Chapters.SelectMany(c => c.Likes).Count()
                })
                .Where(s => s.TotalLikes > 0)
                .OrderByDescending(s => s.TotalLikes)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.Story.StoryID,
                    Title = s.Story.Title,
                    CoverImageUrl = s.Story.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = s.Story.Chapters.Sum(c => c.ViewCount) > 1000,
                    IsNew = s.Story.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Story.Status == StoryStatus.Completed,
                    ChapterCount = s.Story.Chapters.Count,
                    LastUpdated = s.Story.UpdatedAt,
                    AuthorName = s.Story.Author.DisplayName,
                    ViewCount = s.Story.Chapters.Sum(c => c.ViewCount),
                    LikeCount = s.TotalLikes
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        public async Task<List<StoryViewModel>> GetNewStoriesAsync(int count = 12)
        {
            var stories = await _context.Stories
                .Where(s => s.Status == StoryStatus.Active)
                .OrderByDescending(s => s.CreatedAt)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = s.Chapters.Sum(c => c.ViewCount) > 1000,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName,
                    ViewCount = s.Chapters.Sum(c => c.ViewCount),
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count(),
                    Categories = s.Genres
                        .Select(sg => new CategoryViewModel
                        {
                            Id = sg.GenreID,
                            Name = sg.Genre.Name
                        })
                        .ToList(),
                    LatestChapter = s.Chapters
                        .OrderByDescending(c => c.ChapterOrder)
                        .Select(c => new ChapterViewModel
                        {
                            Id = c.ChapterID,
                            ChapterNumber = c.ChapterOrder,
                            Title = c.Title
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        public async Task<List<StoryViewModel>> GetCompletedStoriesAsync(int count = 12)
        {
            var stories = await _context.Stories
                .Where(s => s.Status == StoryStatus.Completed)
                .OrderByDescending(s => s.UpdatedAt)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = s.Chapters.Sum(c => c.ViewCount) > 1000,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = true,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName,
                    ViewCount = s.Chapters.Sum(c => c.ViewCount),
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count()
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            return await _context.Genres
                .Select(g => new CategoryViewModel
                {
                    Id = g.GenreID,
                    Name = g.Name
                })
                .ToListAsync();
        }

        public async Task<List<AuthorViewModel>> GetFollowedAuthorsAsync(int userId, int count = 12)
        {
            var authors = await _context.Users
                .OrderByDescending(u => _context.FollowUsers.Count(f => f.FollowingID == u.UserID))
                .Take(count)
                .Select(u => new AuthorViewModel
                {
                    Id = u.UserID,
                    Name = u.DisplayName,
                    Avatar = u.Avatar ?? "/image/default-avatar.png",
                    TotalStories = u.Stories.Count(s => s.Status == Models.StoryModel.StoryStatus.Active),
                    TotalFollowers = _context.FollowUsers.Count(f => f.FollowingID == u.UserID),
                    IsFollowed = _context.FollowUsers.Any(f => f.FollowerID == userId && f.FollowingID == u.UserID),
                    RecentStories = u.Stories
                        .Where(s => s.Status == StoryStatus.Active)
                        .OrderByDescending(s => s.UpdatedAt)
                        .Take(3)
                        .Select(s => new StoryViewModel
                        {
                            Id = s.StoryID,
                            Title = s.Title,
                            CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                            LastUpdated = s.UpdatedAt
                        })
                        .ToList()
                })
                .ToListAsync();

            // Update image URLs
            foreach (var author in authors)
            {
                author.Avatar = await _blobService.GetSafeImageUrlAsync(author.Avatar);
                foreach (var story in author.RecentStories)
                {
                    story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
                }
            }

            return authors;
        }

        public async Task<List<StoryViewModel>> GetTopFollowedStoriesAsync(int count = 12)
        {
            var stories = await _context.Stories
                .Where(s => s.Status == StoryStatus.Active)
                .Select(s => new
                {
                    Story = s,
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.StoryID)
                })
                .Where(s => s.FollowCount > 0)
                .OrderByDescending(s => s.FollowCount)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.Story.StoryID,
                    Title = s.Story.Title,
                    CoverImageUrl = s.Story.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = s.Story.Chapters.Sum(c => c.ViewCount) > 1000,
                    IsNew = s.Story.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Story.Status == StoryStatus.Completed,
                    ChapterCount = s.Story.Chapters.Count,
                    LastUpdated = s.Story.UpdatedAt,
                    AuthorName = s.Story.Author.DisplayName,
                    ViewCount = s.Story.Chapters.Sum(c => c.ViewCount),
                    LikeCount = s.Story.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = s.FollowCount
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        public async Task<IntroViewModel> GetDashboardDataAsync(int? userId = null)
        {
            var viewModel = new IntroViewModel
            {
                HeaderMessage = "CHÀO MỪNG BẠN ĐẾN VỚI THẾ GIỚI TRUYỆN CỦA CHÚNG TÔI!",
                HotStories = await GetHotStoriesAsync(),
                MostLikedStories = await GetMostLikedStoriesAsync(),
                NewStories = await GetNewStoriesAsync(),
                CompletedStories = await GetCompletedStoriesAsync(),
                AllCategories = await GetAllCategoriesAsync(),
                TopStoriesOfWeek = await GetTopStoriesOfWeekAsync(),
                TopFollowedStories = await GetTopFollowedStoriesAsync(),
                IsAuthenticated = userId.HasValue
            };

            viewModel.CategorySelectList = new SelectList(viewModel.AllCategories, "Id", "Name");

            if (userId.HasValue)
            {
                viewModel.FollowedAuthors = await GetFollowedAuthorsAsync(userId.Value);
            }
            else
            {
                viewModel.FollowedAuthors = new List<AuthorViewModel>();
            }

            return viewModel;
        }
    }
}