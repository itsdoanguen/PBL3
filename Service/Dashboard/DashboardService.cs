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
                    LikeCount = s.Story.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.Story.StoryID)
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
                .Select(s => new
                {
                    Story = s,
                    TotalViews = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0
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
                    LikeCount = s.Story.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.Story.StoryID)
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
                    ViewCount = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0,
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.StoryID)
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
                    IsHot = _context.Chapters.Where(c => c.StoryID == s.Story.StoryID).Sum(c => (int?)c.ViewCount) > 1000,
                    IsNew = s.Story.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Story.Status == StoryStatus.Completed,
                    ChapterCount = s.Story.Chapters.Count,
                    LastUpdated = s.Story.UpdatedAt,
                    AuthorName = s.Story.Author.DisplayName,
                    ViewCount = _context.Chapters.Where(c => c.StoryID == s.Story.StoryID).Sum(c => (int?)c.ViewCount) ?? 0,
                    LikeCount = s.TotalLikes,
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.Story.StoryID)
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
                    IsHot = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) > 1000,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName,
                    ViewCount = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0,
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.StoryID),
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
                    IsHot = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) > 1000,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = true,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName,
                    ViewCount = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0,
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.StoryID)
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

        /// <summary>
        /// Get top authors with most followers (minimum 1 follower required)
        /// </summary>
        /// <param name="userId">Current user ID (used to check follow status)</param>
        /// <param name="count">Maximum number of authors to return</param>
        /// <returns>List of top authors ordered by follower count</returns>
        public async Task<List<AuthorViewModel>> GetFollowedAuthorsAsync(int userId, int count = 20)
        {
            var authors = await _context.Users
                .Where(u => _context.FollowUsers.Any(f => f.FollowingID == u.UserID)) // Only authors who have at least 1 follower
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
                IsAuthenticated = userId.HasValue
            };

            // Set appropriate header message
            if (userId.HasValue)
            {
                var user = await _context.Users.FindAsync(userId.Value);
                var displayName = user?.DisplayName ?? user?.Email ?? "Bạn";
                viewModel.HeaderMessage = $"Chào mừng trở lại, {displayName}!";
                
                // Populate user profile summary
                if (user != null)
                {
                    var avatarUrl = await _blobService.GetSafeImageUrlAsync(user.Avatar ?? string.Empty);
                    viewModel.UserProfile = new UserProfileSummary
                    {
                        UserID = userId.Value,
                        DisplayName = user.DisplayName ?? user.Email,
                        Avatar = avatarUrl,
                        TotalFollowing = _context.FollowUsers.Count(f => f.FollowerID == userId.Value),
                        TotalFollowers = _context.FollowUsers.Count(f => f.FollowingID == userId.Value),
                        TotalStoriesRead = _context.Set<HistoryModel>().Where(h => h.UserID == userId.Value).Select(h => h.StoryID).Distinct().Count()
                    };
                }
            }
            else
            {
                viewModel.HeaderMessage = "CHÀO MỪNG BẠN ĐẾN VỚI THẾ GIỚI TRUYỆN CỦA CHÚNG TÔI!";
            }

            // Load all story collections
            viewModel.HotStories = await GetHotStoriesAsync();
            viewModel.MostLikedStories = await GetMostLikedStoriesAsync();
            viewModel.NewStories = await GetNewStoriesAsync();
            viewModel.CompletedStories = await GetCompletedStoriesAsync();
            viewModel.TopStoriesOfWeek = await GetTopStoriesOfWeekAsync();
            viewModel.TopFollowedStories = await GetTopFollowedStoriesAsync();
            
            // Load categories
            viewModel.AllCategories = await GetAllCategoriesAsync();
            viewModel.CategorySelectList = new SelectList(viewModel.AllCategories, "Id", "Name");

            // Load real database statistics
            viewModel.Statistics = await GetDatabaseStatsAsync();

            // Load personalized content for authenticated users
            if (userId.HasValue)
            {
                // Get followed authors
                viewModel.FollowedAuthors = await GetFollowedAuthorsAsync(userId.Value);
                
                // Get stories from followed authors
                viewModel.FollowingStories = await GetFollowingStoriesAsync(userId.Value);
                
                // Get personalized recommendations
                viewModel.RecommendedStories = await GetRecommendedStoriesAsync(userId.Value);
                
                // Get recent reading history
                viewModel.RecentlyRead = await GetRecentReadingHistoryAsync(userId.Value);
                
                // Get user activity stats
                viewModel.ActivityStats = await GetUserActivityStatsAsync(userId.Value);
                
                // Get favorite categories
                viewModel.FavoriteCategories = await GetFavoriteCategoriesAsync(userId.Value);
            }

            return viewModel;
        }

        private async Task<List<StoryViewModel>> GetFollowingStoriesAsync(int userId, int count = 8)
        {
            var followingAuthors = await _context.FollowUsers
                .Where(f => f.FollowerID == userId)
                .Select(f => f.FollowingID)
                .ToListAsync();

            var stories = await _context.Stories
                .Where(s => followingAuthors.Contains(s.AuthorID) && 
                           (s.Status == StoryStatus.Active || s.Status == StoryStatus.Completed))
                .OrderByDescending(s => s.UpdatedAt)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) > 1000,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName ?? s.Author.Email,
                    ViewCount = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0,
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.StoryID),
                    Description = s.Description,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        private async Task<List<StoryViewModel>> GetRecommendedStoriesAsync(int userId, int count = 8)
        {
            // Get user reading history to find preferred genres
            var userHistory = await _context.Set<HistoryModel>()
                .Where(h => h.UserID == userId)
                .Select(h => h.StoryID)
                .Distinct()
                .ToListAsync();

            if (!userHistory.Any())
            {
                // If no history, return new stories
                return await GetNewStoriesAsync(count);
            }

            // Get preferred genres from user's reading history
            var preferredGenres = await _context.StoryGenres
                .Where(sg => userHistory.Contains(sg.StoryID))
                .Select(sg => sg.GenreID)
                .Distinct()
                .ToListAsync();

            var stories = await _context.Stories
                .Where(s => s.Status == StoryStatus.Active && 
                           !userHistory.Contains(s.StoryID) &&
                           s.Genres.Any(g => preferredGenres.Contains(g.GenreID)))
                .OrderByDescending(s => s.UpdatedAt)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) > 1000,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author.DisplayName ?? s.Author.Email,
                    ViewCount = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0,
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.StoryID),
                    Description = s.Description,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }

        private async Task<List<RecentReadViewModel>> GetRecentReadingHistoryAsync(int userId, int count = 5)
        {
            var recentReads = await _context.Set<HistoryModel>()
                .Where(h => h.UserID == userId && 
                           h.ChapterID.HasValue && 
                           h.ChapterID.Value > 0 &&
                           h.Chapter != null &&
                           h.Story.Status == StoryStatus.Active)
                .OrderByDescending(h => h.LastReadAt)
                .Take(count)
                .Select(h => new RecentReadViewModel
                {
                    StoryID = h.StoryID,
                    StoryTitle = h.Story.Title,
                    CoverImage = h.Story.CoverImage ?? "/image/default-cover.jpg",
                    ChapterID = h.ChapterID.Value,
                    ChapterTitle = h.Chapter.Title,
                    LastRead = h.LastReadAt,
                    TotalChapters = h.Story.Chapters.Count,
                    ReadProgress = h.Chapter != null && h.Story.Chapters.Any() ? 
                        (int)((double)h.Chapter.ChapterOrder / h.Story.Chapters.Count * 100) : 0
                })
                .ToListAsync();

            // Update cover image URLs
            foreach (var read in recentReads)
            {
                read.CoverImage = await _blobService.GetSafeImageUrlAsync(read.CoverImage);
            }

            return recentReads;
        }

        private async Task<UserActivityStats> GetUserActivityStatsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            
            return new UserActivityStats
            {
                TotalStoriesRead = _context.Set<HistoryModel>().Where(h => h.UserID == userId).Select(h => h.StoryID).Distinct().Count(),
                TotalChaptersRead = _context.Set<HistoryModel>().Count(h => h.UserID == userId),
                TotalCommentsPosted = _context.Comments.Count(c => c.UserID == userId),
                TotalLikesGiven = _context.LikeChapters.Count(l => l.UserID == userId),
                JoinedDate = user?.CreatedAt ?? DateTime.Now,
                DaysActive = (DateTime.Now - (user?.CreatedAt ?? DateTime.Now)).Days,
                CurrentStreak = await CalculateReadingStreakAsync(userId),
                LastActiveDate = _context.Set<HistoryModel>()
                    .Where(h => h.UserID == userId)
                    .OrderByDescending(h => h.LastReadAt)
                    .Select(h => h.LastReadAt)
                    .FirstOrDefault()
            };
        }

        private async Task<List<CategorySummary>> GetFavoriteCategoriesAsync(int userId, int count = 5)
        {
            var userStoryIds = await _context.Set<HistoryModel>()
                .Where(h => h.UserID == userId)
                .Select(h => h.StoryID)
                .Distinct()
                .ToListAsync();

            var favoriteCategories = await _context.StoryGenres
                .Where(sg => userStoryIds.Contains(sg.StoryID))
                .GroupBy(sg => sg.GenreID)
                .Select(g => new CategorySummary
                {
                    Id = g.Key,
                    Name = g.First().Genre.Name,
                    StoryCount = g.Count()
                })
                .OrderByDescending(c => c.StoryCount)
                .Take(count)
                .ToListAsync();

            return favoriteCategories;
        }

        private async Task<int> CalculateReadingStreakAsync(int userId)
        {
            var histories = await _context.Set<HistoryModel>()
                .Where(h => h.UserID == userId)
                .OrderByDescending(h => h.LastReadAt)
                .Select(h => h.LastReadAt.Date)
                .Distinct()
                .ToListAsync();

            if (!histories.Any()) return 0;

            int streak = 1;
            var today = DateTime.Now.Date;
            var currentDate = today;

            // Check if user read today or yesterday
            if (histories.First() != today && histories.First() != today.AddDays(-1))
            {
                return 0;
            }

            for (int i = 1; i < histories.Count; i++)
            {
                var previousDate = histories[i - 1];
                var currentHistoryDate = histories[i];

                if (previousDate.AddDays(-1) == currentHistoryDate)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            
            return new DatabaseStats
            {
                TotalActiveStories = await _context.Stories.CountAsync(s => s.Status == StoryStatus.Active),
                TotalNewStories = await _context.Stories.CountAsync(s => s.CreatedAt >= sevenDaysAgo && s.Status == StoryStatus.Active),
                TotalCompletedStories = await _context.Stories.CountAsync(s => s.Status == StoryStatus.Completed),
                TotalCategories = await _context.Genres.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalAuthors = await _context.Stories.Select(s => s.AuthorID).Distinct().CountAsync()
            };
        }

        public async Task<List<StoryViewModel>> GetStoriesFromFollowedAuthorsAsync(int userId, int count = 100)
        {
            var followingAuthors = await _context.FollowUsers
                .Where(f => f.FollowerID == userId)
                .Select(f => f.FollowingID)
                .ToListAsync();

            var stories = await _context.Stories
                .Where(s => followingAuthors.Contains(s.AuthorID) &&
                            (s.Status == StoryStatus.Active || s.Status == StoryStatus.Completed))
                .Include(s => s.Author)
                .OrderByDescending(s => s.UpdatedAt)
                .Take(count)
                .Select(s => new StoryViewModel
                {
                    Id = s.StoryID,
                    Title = s.Title,
                    CoverImageUrl = s.CoverImage ?? "/image/default-cover.jpg",
                    IsHot = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) > 1000,
                    IsNew = s.CreatedAt > DateTime.Now.AddDays(-7),
                    IsCompleted = s.Status == StoryStatus.Completed,
                    ChapterCount = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    AuthorName = s.Author != null ? (s.Author.DisplayName ?? s.Author.Email) : "Unknown",
                    ViewCount = _context.Chapters.Where(c => c.StoryID == s.StoryID).Sum(c => (int?)c.ViewCount) ?? 0,
                    LikeCount = s.Chapters.SelectMany(c => c.Likes).Count(),
                    FollowCount = _context.FollowStories.Count(fs => fs.StoryID == s.StoryID),
                    Description = s.Description,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            // Update image URLs
            foreach (var story in stories)
            {
                story.CoverImageUrl = await _blobService.GetSafeImageUrlAsync(story.CoverImageUrl);
            }

            return stories;
        }
    }
}