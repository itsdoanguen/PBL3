using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Story;
using PBL3.ViewModels.Chapter;
using PBL3.Service;
using System.Security.Claims;

namespace PBL3.Service
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
                CoverImage = story.CoverImage,
                TotalLike = totalLike,
                TotalBookmark = totalBookmark,
                TotalComment = totalComment,
                TotalChapter = chapterList.Count,
                TotalView = totalView,
                Chapters = chapterList
            };
        }

        public async Task<(bool isSuccess, string errorMessage, int storyID)> UpdateStoryStatusAsync(int storyID, int currentUserID, string newStatus)
        {
            return NotImplementedException();
        }
    }
}
