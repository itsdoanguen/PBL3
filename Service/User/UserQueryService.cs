using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.ViewModels.Moderator;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.User
{
    public class UserQueryService : IUserQueryService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public UserQueryService(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }
        public async Task<ViewModels.Moderator.UserProfileViewModel> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new PBL3.ViewModels.Moderator.UserProfileViewModel
                {
                    UserID = u.UserID,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Avatar = u.Avatar,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    Status = u.Status,
                    TotalWarning = u.TotalWarning
                })
                .FirstOrDefaultAsync();
            if (user != null && !string.IsNullOrEmpty(user.Avatar))
            {
                user.Avatar = await _blobService.GetSafeImageUrlAsync(user.Avatar);
            }
            return user ?? new PBL3.ViewModels.Moderator.UserProfileViewModel();
        }
        public async Task<List<UserStoryCardViewModel>> GetUserStoriesAsync(int userId)
        {
            var userStories = await _context.Stories
                .Where(s => s.AuthorID == userId && s.Status != PBL3.Models.StoryModel.StoryStatus.Inactive)
                .Select(s => new UserStoryCardViewModel
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    Cover = s.CoverImage,
                    TotalChapters = s.Chapters.Count,
                    LastUpdated = s.UpdatedAt,
                    Status = s.Status
                })
                .ToListAsync();
            foreach (var story in userStories)
            {
                if (!string.IsNullOrEmpty(story.Cover))
                    story.Cover = await _blobService.GetSafeImageUrlAsync(story.Cover);
            }
            return userStories;
        }
        public async Task<List<CommentViewModel>> GetUserCommentsAsync(int userId)
        {
            var comments = await _context.Comments
                .Where(c => c.UserID == userId)
                .Select(c => new CommentViewModel
                {
                    CommentID = c.CommentID,
                    UserID = c.UserID,
                    ChapterID = c.ChapterID,
                    StoryID = c.StoryID,
                    Content = c.Content,
                    IsDeleted = c.isDeleted,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
            return comments;
        }
    }
}
