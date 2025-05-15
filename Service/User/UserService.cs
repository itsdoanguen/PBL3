using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Service.Discovery;
using PBL3.ViewModels.User;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service.User
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly IStoryRankingService _storyRankingService;
        public UserService(ApplicationDbContext context, BlobService blobService, IStoryRankingService storyRankingService)
        {
            _context = context;
            _blobService = blobService;
            _storyRankingService = storyRankingService;
        }
        public async Task<List<UserStoryCardViewModel>> GetUserStoryCard(int userId)
        {
            var stories = await _context.Stories
                .Where(s => s.AuthorID == userId)
                .Select(s => new UserStoryCardViewModel
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    Cover = s.CoverImage,
                    LastUpdated = s.UpdatedAt,
                    Status = s.Status,
                    TotalChapters = _context.Chapters.Count(c => c.StoryID == s.StoryID)
                })
                .ToListAsync();

            //Lấy URL của ảnh bìa từ blob
            foreach (var story in stories)
            {
                story.Cover = await _blobService.GetSafeImageUrlAsync(story.Cover);
            }
            return stories;
        }


        public async Task<UserProfileViewModel> GetUserProfile(int userId)
        {
            var userInfo = await _context.Users.FindAsync(userId);
            if (userInfo == null)
            {
                return null;
            }

            var stories = await GetUserStoryCard(userId);

            var avatarUrl = await _blobService.GetSafeImageUrlAsync(userInfo.Avatar);
            var bannerUrl = await _blobService.GetSafeImageUrlAsync(userInfo.Banner);

            var profile = new UserProfileViewModel
            {
                DisplayName = userInfo.DisplayName,
                Email = userInfo.Email,
                Avatar = avatarUrl,
                Banner = bannerUrl,
                Bio = userInfo.Bio,
                CreatedAt = userInfo.CreatedAt,
                DateOfBirth = userInfo.DateOfBirth,
                Role = userInfo.Role,
                Gender = userInfo.Gender,
                Status = userInfo.Status,
                TotalUploadedStories = stories.Where(s => s.Status == Models.StoryModel.StoryStatus.Active).Count(),
                Stories = stories,
                TotalFollowers = _context.FollowUsers.Count(f => f.FollowingID == userId),
                TotalFollowings = _context.FollowUsers.Count(f => f.FollowerID == userId),
                TotalComments = _context.Comments.Count(c => c.UserID == userId)
            };

            return profile;

        }


        public async Task<UserIndexViewModel> GetUserIndexViewModelAsync(int userId)
        {
            var userIndexViewModel = new UserIndexViewModel();

            var topStory = await _storyRankingService.GetTopStoriesOfWeekAsync(10);

            if (topStory != null)
            {
                userIndexViewModel.TopStoryInWeek = topStory;
            }

            return userIndexViewModel;
        }

    }
}
