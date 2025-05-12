using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PBL3.Data;
using PBL3.Models;
using PBL3.Service.Chapter;
using PBL3.Service.Image;
using PBL3.ViewModels.Story;

namespace PBL3.Service.Story
{
    public class StoryService : IStoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IChapterService _chapterService;
        private readonly IImageService _imageService;

        public StoryService(ApplicationDbContext context, BlobService blobService, IChapterService chapterService, IImageService imageService)
        {
            _context = context;
            _blobService = blobService;
            _chapterService = chapterService;
            _imageService = imageService;
        }

        public async Task<(bool isSuccess, string errorMessage, int? storyID)> CreateStoryAsync(StoryCreateViewModel model, int authorID)
        {
            var existingStory = await _context.Stories
                .FirstOrDefaultAsync(s => s.Title == model.Title);
            if (existingStory != null)
            {
                return (false, "Tên truyện đã tồn tại", null);
            }

            if (model.GenreIDs == null || !model.GenreIDs.Any())
            {
                return (false, "Thể loại là bắt buộc, hãy chọn ít nhất 1", null);
            }

            string coverImagePath = "/image/default-cover.jpg";

            if (model.UploadCover != null)
            {
                var (uploadSuccess, errorMessage, uploadedUrl) = await _imageService.UploadValidateImageAsync(model.UploadCover, "covers");
                if (!uploadSuccess)
                {
                    return (false, errorMessage, null);
                }

                coverImagePath = uploadedUrl;
            }

            var newStory = new StoryModel
            {
                Title = model.Title,
                Description = model.Description,
                CoverImage = coverImagePath,
                Status = StoryModel.StoryStatus.Inactive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                AuthorID = authorID
            };

            await _context.Stories.AddAsync(newStory);
            await _context.SaveChangesAsync();

            var storyGenres = model.GenreIDs.Select(genreID => new StoryGenreModel
            {
                StoryID = newStory.StoryID,
                GenreID = genreID
            }).ToList();

            await _context.StoryGenres.AddRangeAsync(storyGenres);
            await _context.SaveChangesAsync();

            return (true, "Tạo truyện mới thành công!", newStory.StoryID);
        }

        public async Task<(bool isSuccess, string errorMessage)> DeleteStoryAsync(int storyID, int currentUserID)
        {
            var story = await _context.Stories.FirstOrDefaultAsync(s => s.StoryID == storyID);
            if (story == null)
            {
                return (false, "Truyện không tồn tại");
            }
            if (story.AuthorID != currentUserID)
            {
                return (false, "AccessDenied");
            }

            var relatedCommentsStory = await _context.Comments
                .Where(c => c.StoryID == storyID)
                .ToListAsync();
            _context.Comments.RemoveRange(relatedCommentsStory);

            // Xóa chapter từ IChapterSevice
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyID)
                .ToListAsync();
            foreach (var chapter in chapters)
            {
                await _chapterService.DeleteChapterAsync(chapter.ChapterID, storyID);
            }
            // Xóa Story
            _context.Stories.RemoveRange(story);
            await _context.SaveChangesAsync();

            return (true, "Xóa truyện thành công");
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

        public async Task<(bool isSuccess, string errorMessage)> UpdateStoryAsync(StoryEditViewModel model, int currentUserID)
        {
            var story = await _context.Stories
     .Where(s => s.StoryID == model.StoryID)
     .FirstOrDefaultAsync();
            if (story == null)
            {
                return (false, "Truyện không tồn tại");
            }
            if (story.AuthorID != currentUserID)
            {
                return (false, "AccessDenied");
            }
            if (model.GenreIDs == null || !model.GenreIDs.Any())
            {
                return (false, "Thể loại là bắt buộc, hãy chọn ít nhất 1");
            }

            string coverImagePath = story.CoverImage;
            if (model.UploadCover != null)
            {
                var (uploadSuccess, errorMessage, uploadedUrl) = await _imageService.UploadValidateImageAsync(model.UploadCover, "covers");
                if (!uploadSuccess || string.IsNullOrEmpty(uploadedUrl))
                {
                    return (false, errorMessage);
                }
                coverImagePath = uploadedUrl;
            }

            story.Title = model.Title;
            story.Description = model.Description;
            story.CoverImage = coverImagePath;
            story.UpdatedAt = DateTime.UtcNow;

            _context.Stories.Update(story);
            await _context.SaveChangesAsync();

            var existingGenres = await _context.StoryGenres
                .Where(sg => sg.StoryID == model.StoryID)
                .ToListAsync();
            _context.StoryGenres.RemoveRange(existingGenres);

            var newStoryGenres = model.GenreIDs.Select(genreID => new StoryGenreModel
            {
                StoryID = model.StoryID,
                GenreID = genreID
            }).ToList();

            await _context.StoryGenres.AddRangeAsync(newStoryGenres);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật truyện thành công");
        }

        public async Task<(bool isSuccess, string errorMessage, int storyID)> UpdateStoryStatusAsync(int storyID, int currentUserID, string newStatus)
        {
            var story = await _context.Stories
                .Where(s => s.StoryID == storyID)
                .FirstOrDefaultAsync();
            if (story == null)
            {
                return (false, "Truyện không tồn tại", 0);
            }

            var authorID = await _context.Stories
                .Where(s => s.StoryID == storyID)
                .Select(s => s.AuthorID)
                .FirstOrDefaultAsync();
            if (authorID != currentUserID)
            {
                return (false, "AccessDenied", 0);
            }

            if (!Enum.TryParse<StoryModel.StoryStatus>(newStatus, out var parsedStatus))
            {
                return (false, "Trạng thái không hợp lệ", 0);
            }

            story.Status = parsedStatus;
            story.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Cập nhật trạng thái truyện thành công", story.StoryID);
        }

        public async Task<StoryDetailViewModel> GetStoryDetailAsync(int storyID, int currentUserID)
        {

            var story = await _context.Stories.Where(s => s.StoryID == storyID && s.Status == StoryModel.StoryStatus.Active || s.Status == StoryModel.StoryStatus.Completed).FirstOrDefaultAsync();
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

            var viewModel = new StoryDetailViewModel
            {
                StoryID = story.StoryID,
                StoryName = story.Title,
                StoryDescription = story.Description,
                StoryImage = await _blobService.GetSafeImageUrlAsync(story.CoverImage),
                LastUpdated = story.UpdatedAt,
                StoryStatus = story.Status,
                Author = author,
                gerneVMs = GetGerneForStory(storyID),
                TotalChapter = await _context.Chapters.CountAsync(c => c.StoryID == storyID && c.Status == ChapterStatus.Active),
                TotalComment = await _context.Comments.CountAsync(c => c.StoryID == storyID),
                TotalView = await _context.Chapters.Where(c => c.StoryID == storyID).SumAsync(c => c.ViewCount),
                TotalFollow = await _context.FollowStories.CountAsync(f => f.StoryID == storyID),
                TotalWord = await GetTotalStoryWordAsync(storyID),
                TotalBookmark = await _context.Bookmarks.CountAsync(b => b.Chapter.StoryID == storyID),
                Comments = GetCommentForStory(storyID),
                Chapters = GetChapterForStory(storyID),
                IsFollowed = await _context.FollowStories
                    .AnyAsync(f => f.StoryID == storyID && f.UserID == currentUserID),
                Rating = await RatingStoryAsync(storyID)
            };

            return viewModel;
        }


        private List<CommentInfo> GetCommentForStory(int storyID)
        {
            var comments = _context.Comments
                .Where(c => c.StoryID == storyID)
                .Select(c => new CommentInfo
                {
                    CommentID = c.CommentID,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    User = new UserInfo
                    {
                        UserID = c.User.UserID,
                        UserName = c.User.DisplayName,
                        UserAvatar = c.User.Avatar
                    }
                })
                .ToList();

            return comments;
        }
        private List<GerneVM> GetGerneForStory(int storyID)
        {
            var genres = _context.StoryGenres
                .Where(sg => sg.StoryID == storyID)
                .Select(sg => new GerneVM
                {
                    GenreID = sg.Genre.GenreID,
                    Name = sg.Genre.Name
                })
                .ToList();
            return genres;
        }
        private List<ChapterInfo> GetChapterForStory(int storyID)
        {
            var chapters = _context.Chapters
                .Where(c => c.StoryID == storyID && c.Status == ChapterStatus.Active).OrderBy(c=> c.ChapterOrder)
                .Select(c => new ChapterInfo
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    UpdatedAt = c.UpdatedAt
                })
                .ToList();
            return chapters;
        }
        private async Task<int> GetTotalStoryWordAsync(int storyID)
        {
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyID)
                .Select(c => c.Content)
                .ToListAsync();

            int totalWord = chapters.Sum(content => _chapterService.CountWordsInChapter(content));
            return totalWord;
        }

        private async Task<double> RatingStoryAsync(int storyID)
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

            int totalLike = 0;
            foreach (var c in chapters)
            {
                totalLike += await _context.LikeChapters.CountAsync(l => l.ChapterID == c.ChapterID);
            }

            int totalView = chapters.Sum(c => c.ViewCount);
            int totalWord = await GetTotalStoryWordAsync(storyID);

            double averageLike = (double)totalLike / totalChapter;
            double averageView = (double)totalView / totalChapter;
            double averageWord = (double)totalWord / totalChapter;

            double rating = averageLike * 0.5 + averageView * 0.2 + averageWord * 0.3;
            return rating;
        }
    }
}
