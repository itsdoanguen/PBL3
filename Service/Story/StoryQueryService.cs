using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;
using PBL3.ViewModels.Story;
using PBL3.Service.Chapter;
using PBL3.Service.Comment;
using PBL3.Models;

namespace PBL3.Service.Story
{
    public class StoryQueryService : IStoryQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IChapterService _chapterService;
        private readonly ICommentService _commentService;
        public StoryQueryService(ApplicationDbContext context, BlobService blobService, IChapterService chapterService, ICommentService commentService)
        {
            _context = context;
            _blobService = blobService;
            _chapterService = chapterService;
            _commentService = commentService;
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

        public async Task<StoryEditViewModel> GetStoryDetailForEditAsync(int storyID, int currentAuthorID)
        {
            var story = await _context.Stories
                .Where(s => s.AuthorID == currentAuthorID && s.StoryID == storyID)
                .FirstOrDefaultAsync();

            if (story == null)
            {
                return null;
            }

            var chapterList = await _chapterService.GetChaptersForStoryAsync(storyID);

            var totalLike = await _context.LikeChapters
                .Where(l => l.Chapter.StoryID == storyID)
                .CountAsync();

            var totalBookmark = await _context.Bookmarks
                .Where(b => b.Chapter.StoryID == storyID)
                .CountAsync();

            var totalComment = await _context.Comments
                .Where(c => c.Chapter.StoryID == storyID)
                .CountAsync();

            var totalView = await _context.Chapters
                .Where(c => c.StoryID == storyID)
                .SumAsync(c => c.ViewCount);

            return new StoryEditViewModel
            {
                StoryID = story.StoryID,
                Title = story.Title,
                Description = story.Description,
                CoverImage = await _blobService.GetSafeImageUrlAsync(story.CoverImage),
                TotalLike = totalLike,
                TotalBookmark = totalBookmark,
                TotalComment = totalComment,
                TotalChapter = chapterList.Count,
                TotalView = totalView,
                Chapters = chapterList,
                StoryStatus = story.Status,
                GenreIDs = await _context.StoryGenres
                    .Where(sg => sg.StoryID == storyID)
                    .Select(sg => sg.GenreID)
                    .ToListAsync(),
                AvailableGenres = await _context.Genres.Select(g => new GerneVM
                {
                    GenreID = g.GenreID,
                    Name = g.Name
                }).ToListAsync()
            };
        }

        public async Task<StoryDetailViewModel> GetStoryDetailAsync(int storyID, int currentUserID)
        {
            var story = await _context.Stories.Where(s => s.StoryID == storyID && (s.Status == StoryModel.StoryStatus.Active || s.Status == StoryModel.StoryStatus.Completed)).FirstOrDefaultAsync();
            if (story == null)
            {
                return null;
            }
            var author = await _context.Users
                .Where(u => u.UserID == story.AuthorID)
                .Select(u => new UserInfo
                {
                    UserID = u.UserID,
                    UserName = u.DisplayName,
                    UserAvatar = u.Avatar
                })
                .FirstOrDefaultAsync();
            author.UserAvatar = await _blobService.GetSafeImageUrlAsync(author.UserAvatar);

            var viewModel = new StoryDetailViewModel
            {
                StoryID = story.StoryID,
                StoryName = story.Title,
                StoryDescription = (story.Description ?? "Chưa có mô tả").Replace("\n", "<br/>"),
                StoryImage = await _blobService.GetSafeImageUrlAsync(story.CoverImage),
                LastUpdated = story.UpdatedAt,
                StoryStatus = story.Status,
                Author = author,
                gerneVMs = await GetGerneForStoryAsync(storyID),
                TotalChapter = await _context.Chapters.CountAsync(c => c.StoryID == storyID && c.Status == ChapterStatus.Active),
                TotalComment = await _context.Comments.CountAsync(c => c.StoryID == storyID),
                TotalView = await _context.Chapters.Where(c => c.StoryID == storyID).SumAsync(c => c.ViewCount),
                TotalFollow = await _context.FollowStories.CountAsync(f => f.StoryID == storyID),
                TotalWord = await GetTotalStoryWordAsync(storyID),
                TotalBookmark = await _context.Bookmarks.CountAsync(b => b.Chapter.StoryID == storyID),
                Comments = await _commentService.GetCommentsAsync("story", storyID),
                Chapters = await GetChapterForStoryAsync(storyID),
                IsFollowed = await _context.FollowStories
                    .AnyAsync(f => f.StoryID == storyID && f.UserID == currentUserID),
                Rating = await RatingStoryAsync(storyID),
                LastReadAt = await LastReadAtAsync(currentUserID, storyID)
            };

            return viewModel;
        }

        public async Task<List<GerneVM>> GetGerneForStoryAsync(int storyID)
        {
            var genres = await _context.StoryGenres
                .Where(sg => sg.StoryID == storyID)
                .Select(sg => new GerneVM
                {
                    GenreID = sg.Genre.GenreID,
                    Name = sg.Genre.Name
                })
                .ToListAsync();
            return genres;
        }

        public async Task<List<ChapterInfo>> GetChapterForStoryAsync(int storyID)
        {
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyID && c.Status == ChapterStatus.Active).OrderBy(c => c.ChapterOrder)
                .Select(c => new ChapterInfo
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
            return chapters;
        }

        public async Task<int> GetTotalStoryWordAsync(int storyID)
        {
            var chapters = await _context.Chapters.Where(c => c.StoryID == storyID).ToListAsync();
            int totalWords = 0;
            foreach (var chapter in chapters)
            {
                if (!string.IsNullOrEmpty(chapter.Content))
                {
                    totalWords += chapter.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                }
            }
            return totalWords;
        }

        public async Task<double> RatingStoryAsync(int storyID)
        {
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyID)
                .Select(c => new
                {
                    c.ChapterID,
                    c.ViewCount
                })
                .ToListAsync();

            int totalChapter = chapters.Count;
            if (totalChapter == 0) return 0;

            // Tính tổng số lượt thích trực tiếp trong một truy vấn duy nhất
            int totalLike = await _context.LikeChapters
                .Where(l => chapters.Select(c => c.ChapterID).Contains(l.ChapterID))
                .CountAsync();

            // Tính tổng lượt xem
            int totalView = chapters.Sum(c => c.ViewCount);

            // Lấy tổng số từ trong truyện
            int totalWord = await GetTotalStoryWordAsync(storyID);

            // Tính các giá trị trung bình
            double averageLike = (double)totalLike / totalChapter;
            double averageView = (double)totalView / totalChapter;
            double averageWord = (double)totalWord / totalChapter;

            // Điều chỉnh lại trọng số cho hợp lý, nếu cần
            double rating = averageLike * 0.5 + averageView * 0.2 + averageWord * 0.3;

            // Đảm bảo rating không vượt quá 10 hoặc dưới 0
            rating = Math.Max(0, Math.Min(rating, 10)); // Giới hạn rating từ 0 đến 10

            return rating;
        }
        private async Task<int> LastReadAtAsync(int userID, int storyID)
        {
            var history = await _context.Set<HistoryModel>()
                .Where(h => h.UserID == userID && h.StoryID == storyID)
                .OrderByDescending(h => h.LastReadAt)
                .FirstOrDefaultAsync();
            return history?.ChapterID ?? 0; 
        }
    }
}
