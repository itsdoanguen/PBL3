﻿using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.Service.Bookmark;
using PBL3.Service.Comment;
using PBL3.Service.Like;
using PBL3.Service.Notification;
using PBL3.Service.Style;
using PBL3.ViewModels.Chapter;

namespace PBL3.Service.Chapter
{
    public class ChapterService : IChapterService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILikeChapterService _likeChapterService;
        private readonly IStyleService _styleService;
        private readonly ICommentService _commentService;
        private readonly IBookmarkService _bookmarkService;
        private readonly INotificationService _notificationService;
        public ChapterService(ApplicationDbContext context, ILikeChapterService likeChapterService, IStyleService styleService, ICommentService commentService, IBookmarkService bookmarkService, INotificationService notificationService)
        {
            _context = context;
            _likeChapterService = likeChapterService;
            _styleService = styleService;
            _commentService = commentService;
            _bookmarkService = bookmarkService;
            _notificationService = notificationService;
        }
        //Lấy thông tin chi tiết của chapter
        public async Task<ChapterDetailViewModel> GetChapterDetailAsync(int chapterId, string currentUserId, Func<string, bool> checkCookieExists, Action<string, string, CookieOptions> setCookie)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Story)
                .Include(c => c.Comments)
                .FirstOrDefaultAsync(c => c.ChapterID == chapterId);

            if (chapter == null)
                return null;

            var storyStatus = await _context.Stories
                .Where(s => s.StoryID == chapter.StoryID)
                .Select(s => s.Status)
                .FirstOrDefaultAsync();

            if (storyStatus == StoryModel.StoryStatus.Inactive || storyStatus == StoryModel.StoryStatus.Locked || storyStatus == StoryModel.StoryStatus.ReviewPending)
                return null;

            if (chapter.Status == ChapterStatus.Inactive)
                return null;

            // Xử lý ViewCount và Cookie
            string cookieName = currentUserId != null ? $"viewedchapter{chapterId}_user_{currentUserId}" : $"viewedchapter{chapterId}_guest";

            if (!checkCookieExists(cookieName))
            {
                chapter.ViewCount++;
                _context.Update(chapter);
                await _context.SaveChangesAsync();

                var options = new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddMinutes(10),
                    HttpOnly = true,
                    IsEssential = true
                };
                setCookie(cookieName, "true", options);
            }

            var userStyle = await _context.Styles
                .FirstOrDefaultAsync(s => s.UserID == int.Parse(currentUserId));
            if (userStyle == null)
            {
                userStyle = await _styleService.InitStyleForUserAsync(int.Parse(currentUserId));
            }
            var StyleViewModel = new StyleViewModel
            {
                StyleID = userStyle.StyleID,
                UserID = userStyle.UserID,
                FontFamily = userStyle.FontFamily,
                FontSize = userStyle.FontSize,
                BackgroundColor = userStyle.BackgroundColor
            };

            int parsedUserId = 0;
            if (!string.IsNullOrEmpty(currentUserId))
                int.TryParse(currentUserId, out parsedUserId);

            var viewModel = new ChapterDetailViewModel
            {
                ChapterID = chapter.ChapterID,
                Title = chapter.Title,
                Content = chapter.Content,
                CreatedAt = chapter.CreatedAt,
                UpdatedAt = chapter.UpdatedAt,
                ViewCount = chapter.ViewCount,
                TotalWord = CountWordsInChapter(chapter.Content),
                StoryTitle = chapter.Story?.Title ?? "Không rõ",
                StoryID = chapter.StoryID,


                Comments = await _commentService.GetCommentsAsync("chapter", chapter.ChapterID),
                NextChapterID = await GetNextChapter(chapter.ChapterID, chapter.StoryID),
                PreviousChapterID = await GetPreviousChapter(chapter.ChapterID, chapter.StoryID),
                ChapterList = await GetChapterList(chapter.StoryID),
                IsLikedByCurrentUser = parsedUserId > 0 ? await _likeChapterService.IsLikedByCurrentUserAsync(chapter.ChapterID, parsedUserId) : false,
                LikeCount = await _likeChapterService.GetLikeCountAsync(chapter.ChapterID),
                Style = StyleViewModel,
                IsBookmarkedByCurrentUser = parsedUserId > 0 ? await _bookmarkService.IsBookmarkedAsync(parsedUserId, chapter.ChapterID) : false
            };

            return viewModel;
        }

        //Tạo chapter mới
        public async Task<ChapterModel> CreateChapterAsync(ChapterCreateViewModel chapterViewModel)
        {
            var lastChapterOrder = await _context.Chapters
                .Where(c => c.StoryID == chapterViewModel.StoryID)
                .OrderByDescending(c => c.ChapterOrder)
                .Select(c => c.ChapterOrder)
                .FirstOrDefaultAsync();

            var newChapter = new ChapterModel
            {
                Title = chapterViewModel.Title,
                StoryID = chapterViewModel.StoryID,
                Content = "",
                Status = ChapterStatus.Inactive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ViewCount = 0,
                ChapterOrder = lastChapterOrder + 1
            };

            await _context.Chapters.AddAsync(newChapter);
            await _context.SaveChangesAsync();

            return newChapter;
        }

        //Xóa chapter
        public async Task DeleteChapterAsync(int chapterId, int storyId)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter == null)
            {
                return;
            }

            // Xóa comment liên quan
            var relatedComments = _context.Comments.Where(c => c.ChapterID == chapterId);
            _context.Comments.RemoveRange(relatedComments);

            // Xóa history liên quan
            var relatedHistories = _context.Set<HistoryModel>().Where(h => h.ChapterID == chapterId);
            _context.Set<HistoryModel>().RemoveRange(relatedHistories);

            // Xóa notification liên quan
            var relatedNotifications = _context.Notifications.Where(n => n.ChapterID == chapterId);
            _context.Notifications.RemoveRange(relatedNotifications);

            // Cập nhật lại ChapterOrder cho các chương sau
            var chapterToUpdate = await _context.Chapters
                .Where(c => c.ChapterOrder > chapter.ChapterOrder && c.StoryID == storyId)
                .ToListAsync();
            foreach (var c in chapterToUpdate)
            {
                c.ChapterOrder--;
            }

            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
        }

        //Lấy dữ liệu chapter cho edit
        public async Task<ChapterEditViewModel> GetChapterForEditAsync(int chapterId, int storyId, int currentUserId)
        {
            var authorId = await _context.Stories
                .Where(s => s.StoryID == storyId)
                .Select(s => s.AuthorID)
                .FirstOrDefaultAsync();

            if (authorId != currentUserId)
            {
                return new ChapterEditViewModel();
            }

            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.ChapterID == chapterId && c.StoryID == storyId);

            if (chapter == null)
            {
                return new ChapterEditViewModel();
            }

            return new ChapterEditViewModel
            {
                ChapterID = chapterId,
                StoryID = storyId,
                Title = chapter.Title,
                Content = chapter.Content,
                ChapterStatus = chapter.Status
            };
        }

        //Cập nhật chapter
        public async Task<bool> UpdateChapterAsync(ChapterEditViewModel model)
        {
            var chapter = await _context.Chapters.FindAsync(model.ChapterID);
            if (chapter == null)
            {
                return false;
            }

            chapter.Title = model.Title ?? chapter.Title;
            chapter.Content = model.Content ?? chapter.Content;
            chapter.UpdatedAt = DateTime.Now;

            var story = await _context.Stories.FirstOrDefaultAsync(s => s.StoryID == model.StoryID);
            if (story != null)
            {
                story.UpdatedAt = DateTime.Now;
            }


            await _context.SaveChangesAsync();
            return true;
        }

        //Chỉnh sửa trạng thái chapter
        public async Task<(bool Success, string Message, int StoryId)> UpdateChapterStatusAsync(int chapterId, int currentUserId, string newStatus)
        {
            var chapter = await _context.Chapters.FindAsync(chapterId);
            if (chapter == null)
            {
                return (false, "Chương không tồn tại.", 0);
            }

            var authorId = await _context.Stories
                .Where(s => s.StoryID == chapter.StoryID)
                .Select(s => s.AuthorID)
                .FirstOrDefaultAsync();

            if (authorId != currentUserId)
            {
                return (false, "AccessDenied", chapter.StoryID);
            }

            var storyStatus = await _context.Stories
                .Where(s => s.StoryID == chapter.StoryID)
                .Select(s => s.Status)
                .FirstOrDefaultAsync();
            if (storyStatus == StoryModel.StoryStatus.Inactive)
            {
                return (false, "Truyện chưa được xuất bản, không thể xuất bản chương!", chapter.StoryID);
            }
            if (storyStatus == StoryModel.StoryStatus.ReviewPending)
            {
                return (false, "Truyện đang chờ duyệt, không thể xuất bản chương!", chapter.StoryID);
            }
            if (storyStatus == StoryModel.StoryStatus.Locked)
            {
                return (false, "Truyện đã bị khóa, không thể xuất bản chương!", chapter.StoryID);
            }

            if (!Enum.TryParse<ChapterStatus>(newStatus, out var parsedStatus))
            {
                return (false, "Trạng thái không hợp lệ.", chapter.StoryID);
            }

            var oldStatus = chapter.Status;
            chapter.Status = parsedStatus;
            chapter.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Gửi notification khi xuất bản chương
            if (oldStatus == ChapterStatus.Inactive && parsedStatus == ChapterStatus.Active)
            {
                await _notificationService.InitNewChapterNotificationAsync(chapter.StoryID, chapter.ChapterID, authorId);
            }

            return (true, "Cập nhật trạng thái chương thành công.", chapter.StoryID);
        }

        public async Task<List<ChapterSummaryViewModel>> GetChaptersForStoryAsync(int storyId)
        {
            return await _context.Chapters
                .Where(c => c.StoryID == storyId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new ChapterSummaryViewModel
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ViewCount = c.ViewCount,
                    ChapterOrder = c.ChapterOrder,
                    Status = (ChapterSummaryViewModel.ChapterStatus)c.Status
                })
                .ToListAsync();
        }


        //METHOD
        private async Task<List<ChapterList>> GetChapterList(int storyID)
        {
            return await _context.Chapters
                .Where(c => c.StoryID == storyID && c.Status == ChapterStatus.Active)
                .OrderBy(c => c.ChapterOrder)
                .Select(c => new ChapterList
                {
                    ChapterID = c.ChapterID,
                    Title = c.Title
                })
                .ToListAsync();
        }

        private async Task<int> GetNextChapter(int chapterID, int storyID)
        {
            var currentChapterOrder = await _context.Chapters
                .Where(c => c.ChapterID == chapterID && c.StoryID == storyID)
                .Select(c => c.ChapterOrder)
                .FirstOrDefaultAsync();

            var nextChapter = await _context.Chapters
                .Where(c => c.StoryID == storyID && c.ChapterOrder > currentChapterOrder && c.Status == ChapterStatus.Active)
                .OrderBy(c => c.ChapterOrder)
                .FirstOrDefaultAsync();

            return nextChapter?.ChapterID ?? -1;
        }

        private async Task<int> GetPreviousChapter(int chapterID, int storyID)
        {
            var currentChapterOrder = await _context.Chapters
                .Where(c => c.ChapterID == chapterID && c.StoryID == storyID)
                .Select(c => c.ChapterOrder)
                .FirstOrDefaultAsync();

            var previousChapter = await _context.Chapters
                .Where(c => c.StoryID == storyID && c.ChapterOrder < currentChapterOrder && c.Status == ChapterStatus.Active)
                .OrderByDescending(c => c.ChapterOrder)
                .FirstOrDefaultAsync();

            return previousChapter?.ChapterID ?? -1;
        }
        public int CountWordsInChapter(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return 0;
            }

            var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }
        public async Task<bool> UpdateChapterOrderAsync(int chapterId, int storyId, int newOrder)
        {
            var chapters = await _context.Chapters
                .Where(c => c.StoryID == storyId)
                .OrderBy(c => c.ChapterOrder)
                .ToListAsync();
            var chapter = chapters.FirstOrDefault(c => c.ChapterID == chapterId);
            if (chapter == null) return false;
            if (chapter.Status == ChapterStatus.Inactive)
                return false;
            int oldOrder = chapter.ChapterOrder;
            if (newOrder == oldOrder) return true;
            if (newOrder < 1) newOrder = 1;
            if (newOrder > chapters.Count) newOrder = chapters.Count;
            if (newOrder < oldOrder)
            {
                foreach (var c in chapters.Where(c => c.ChapterOrder >= newOrder && c.ChapterOrder < oldOrder))
                    c.ChapterOrder++;
            }
            else
            {
                foreach (var c in chapters.Where(c => c.ChapterOrder > oldOrder && c.ChapterOrder <= newOrder))
                    c.ChapterOrder--;
            }
            chapter.ChapterOrder = newOrder;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
