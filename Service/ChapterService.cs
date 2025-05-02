using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Chapter;

namespace PBL3.Service
{
    public class ChapterService : IChapterService
    {
        private readonly ApplicationDbContext _context;
        public ChapterService(ApplicationDbContext context)
        {
            _context = context;
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

            if (storyStatus == StoryModel.StoryStatus.Inactive)
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
                    Expires = DateTimeOffset.UtcNow.AddMinutes(1),
                    HttpOnly = true,
                    IsEssential = true
                };
                setCookie(cookieName, "true", options);
            }

            return new ChapterDetailViewModel
            {
                ChapterID = chapter.ChapterID,
                Title = chapter.Title,
                Content = chapter.Content,
                CreatedAt = chapter.CreatedAt,
                ViewCount = chapter.ViewCount,
                StoryTitle = chapter.Story?.Title ?? "Không rõ",
                StoryID = chapter.StoryID,
                Comments = chapter.Comments.OrderByDescending(c => c.CreatedAt).ToList(),
                NextChapterID = await GetNextChapter(chapter.ChapterID, chapter.StoryID),
                PreviousChapterID = await GetPreviousChapter(chapter.ChapterID, chapter.StoryID),
                ChapterList = await GetChapterList(chapter.StoryID)
            };
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

            var relatedComments = _context.Comments
                .Where(c => c.ChapterID == chapterId);
            _context.Comments.RemoveRange(relatedComments);

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
                return null; // User không có quyền
            }

            var chapter = await _context.Chapters
                .FirstOrDefaultAsync(c => c.ChapterID == chapterId && c.StoryID == storyId);

            if (chapter == null)
            {
                return null; // Không tìm thấy
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

            chapter.Title = model.Title;
            chapter.Content = model.Content;
            chapter.UpdatedAt = DateTime.UtcNow;

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

            if (!Enum.TryParse<ChapterStatus>(newStatus, out var parsedStatus))
            {
                return (false, "Trạng thái không hợp lệ.", chapter.StoryID);
            }

            chapter.Status = parsedStatus;
            chapter.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

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
    }
}
