using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.Service.Chapter;
using PBL3.Service.Image;
using PBL3.Service.Notification;
using PBL3.Service.Report;
using PBL3.ViewModels.Story;

namespace PBL3.Service.Story
{
    public class StoryService : IStoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IChapterService _chapterService;
        private readonly IImageService _imageService;
        private readonly INotificationService _notificationService;
        private readonly IReportService _reportService;

        public StoryService(ApplicationDbContext context, IChapterService chapterService, IImageService imageService, INotificationService notificationService, IReportService reportService)
        {
            _context = context;
            _chapterService = chapterService;
            _imageService = imageService;
            _notificationService = notificationService;
            _reportService = reportService;
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

            // Xóa chapter (và các liên kết liên quan) qua IChapterService
            var chapters = await _context.Chapters.Where(c => c.StoryID == storyID).ToListAsync();
            foreach (var chapter in chapters)
            {
                await _chapterService.DeleteChapterAsync(chapter.ChapterID, storyID);
            }

            // Xóa comment liên quan đến story
            var relatedComments = _context.Comments.Where(c => c.StoryID == storyID);
            _context.Comments.RemoveRange(relatedComments);

            // Xóa notification liên quan
            var relatedNotifications = _context.Notifications.Where(n => n.StoryID == storyID);
            _context.Notifications.RemoveRange(relatedNotifications);

            // Xóa history liên quan
            var relatedHistories = _context.Set<HistoryModel>().Where(h => h.StoryID == storyID);
            _context.Set<HistoryModel>().RemoveRange(relatedHistories);

            // Xóa genre liên quan
            var relatedGenres = _context.StoryGenres.Where(g => g.StoryID == storyID);
            _context.StoryGenres.RemoveRange(relatedGenres);

            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();

            return (true, "Xóa truyện thành công");
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
            story.UpdatedAt = DateTime.Now;

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

            if (story.Status == StoryModel.StoryStatus.Locked || story.Status == StoryModel.StoryStatus.ReviewPending)
            {
                return (false, "Truyện đang bị khóa, không thể cập nhật trạng thái", 0);
            }

            if (!Enum.TryParse<StoryModel.StoryStatus>(newStatus, out var parsedStatus))
            {
                return (false, "Trạng thái không hợp lệ", 0);
            }

            var oldStatus = story.Status;
            story.Status = parsedStatus;
            story.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            if (oldStatus == StoryModel.StoryStatus.Inactive && parsedStatus == StoryModel.StoryStatus.Active)
            {
                await _notificationService.InitNewStoryNotificationAsync(story.StoryID, authorID);
            }

            return (true, "Cập nhật trạng thái truyện thành công", story.StoryID);
        }

        public async Task<(bool isSuccess, string errorMessage)> LockStoryAsync(int storyID, string message, int moderatorId)
        {
            var storyModel = await _context.Stories
                .Where(s => s.StoryID == storyID)
                .FirstOrDefaultAsync();
            if (storyModel == null)
            {
                return (false, "Truyện không tồn tại");
            }

            storyModel.Status = StoryModel.StoryStatus.Locked;
            storyModel.UpdatedAt = DateTime.Now;

            _context.Stories.Update(storyModel);
            await _context.SaveChangesAsync();

            await _notificationService.InitNewMessageFromModeratorAsync(storyModel.AuthorID,"Thông báo: " + message, moderatorId);
            return (true, "Khóa truyện thành công");
        }
        public async Task<(bool isSuccess, string errorMessage)> UnlockStoryAsync(int storyID, bool isAccepted, string message, int moderatorId)
        {
            var storyModel = await _context.Stories
                .Where(s => s.StoryID == storyID)
                .FirstOrDefaultAsync();
            if (storyModel == null)
            {
                return (false, "Truyện không tồn tại");
            }
            if (isAccepted && storyModel.Status == StoryModel.StoryStatus.ReviewPending)
            {
                storyModel.Status = StoryModel.StoryStatus.Active;
                await _notificationService.InitNewMessageFromModeratorAsync(storyModel.AuthorID, "Thông báo: " + message, moderatorId);
            }
            else
            {
                storyModel.Status = StoryModel.StoryStatus.Locked;
                await _notificationService.InitNewMessageFromModeratorAsync(storyModel.AuthorID, "Thông báo: " + message, moderatorId);
            }
            _context.Stories.Update(storyModel);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật trạng thái truyện thành công");
        }
        public async Task<(bool isSuccess, string errorMessage)> PendingReviewAsync(int storyID, int currentUserId)
        {
            var story = _context.Stories.FirstOrDefault(s => s.StoryID == storyID);
            if (story == null)
            {
                return (false, "Truyện không tồn tại");
            }
            if (story.AuthorID != currentUserId)
            {
                return (false, "AccessDenied");
            }
            var isSubmited = await _context.Notifications.FindAsync(storyID);
            if (isSubmited != null)
            {
                return (false, "Truyện đã được gửi yêu cầu duyệt trước đó");
            }
            story.Status = StoryModel.StoryStatus.ReviewPending;
            story.UpdatedAt = DateTime.Now;
            _context.Stories.Update(story);
            _context.SaveChanges();
            await _reportService.InitReportStoryNotificationAsync(storyID, currentUserId, "Yêu cầu duyệt truyện đăng trở lại");
            return (true, "Đã gửi yêu câu duyệt thành công");
        }
    }
}
