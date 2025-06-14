using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.Service.Notification;
using PBL3.ViewModels.FollowStory;
using PBL3.ViewModels.FollowUser;

namespace PBL3.Service.Follow
{
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly BlobService _blobService;
        public FollowService(ApplicationDbContext context, INotificationService notificationService, BlobService blobService)
        {
            _context = context;
            _notificationService = notificationService;
            _blobService = blobService;
        }

        public async Task<(bool isSuccess, string Message)> FollowStoryAsync(int userId, int storyId)
        {
            var exists = await _context.FollowStories.AnyAsync(f => f.UserID == userId && f.StoryID == storyId);
            if (exists)
                return (false, "Bạn đã theo dõi truyện này rồi.");
            var follow = new FollowStoryModel { UserID = userId, StoryID = storyId };
            _context.FollowStories.Add(follow);
            await _context.SaveChangesAsync();
            return (true, "Theo dõi truyện thành công.");
        }

        public async Task<(bool isSuccess, string Message)> UnfollowStoryAsync(int userId, int storyId)
        {
            var follow = await _context.FollowStories.FirstOrDefaultAsync(f => f.UserID == userId && f.StoryID == storyId);
            if (follow == null)
                return (false, "Bạn chưa theo dõi truyện này.");
            _context.FollowStories.Remove(follow);
            await _context.SaveChangesAsync();
            return (true, "Bỏ theo dõi truyện thành công.");
        }

        public async Task<bool> IsFollowingStoryAsync(int userId, int storyId)
        {
            return await _context.FollowStories.AnyAsync(f => f.UserID == userId && f.StoryID == storyId);
        }

        public async Task<int> CountStoryFollowersAsync(int storyId)
        {
            return await _context.FollowStories.CountAsync(f => f.StoryID == storyId);
        }

        public async Task<FollowStoryViewModel> GetFollowStoryList(int userid)
        {
            var items = await (from f in _context.FollowStories
                               join s in _context.Stories on f.StoryID equals s.StoryID
                               where f.UserID == userid && (s.Status == StoryModel.StoryStatus.Active || s.Status == StoryModel.StoryStatus.ReviewPending)
                               select new FollowStoryItemsViewModel
                               {
                                   StoryID = s.StoryID,
                                   StoryTitle = s.Title,
                                   StoryCoverImageUrl = s.CoverImage
                               }).ToListAsync();

            foreach (var item in items)
            {
                item.StoryCoverImageUrl = await _blobService.GetSafeImageUrlAsync(item.StoryCoverImageUrl ?? string.Empty);
            }
            return new FollowStoryViewModel
            {
                UserID = userid,
                FollowedStories = items
            };
        }


        // USER FOLLOW
        public async Task<(bool isSuccess, string Message)> FollowUserAsync(int followerId, int followingId)
        {
            if (followerId == followingId)
                return (false, "Bạn không thể theo dõi chính mình.");
            var exists = await _context.FollowUsers.AnyAsync(f => f.FollowerID == followerId && f.FollowingID == followingId);
            if (exists)
                return (false, "Bạn đã theo dõi người này rồi.");
            var follow = new FollowUserModel { FollowerID = followerId, FollowingID = followingId };
            _context.FollowUsers.Add(follow);
            await _context.SaveChangesAsync();
            await _notificationService.InitNewFollowNotificationAsync(followerId, followingId);
            return (true, "Theo dõi người dùng thành công.");
        }

        public async Task<(bool isSuccess, string Message)> UnfollowUserAsync(int followerId, int followingId)
        {
            var follow = await _context.FollowUsers.FirstOrDefaultAsync(f => f.FollowerID == followerId && f.FollowingID == followingId);
            if (follow == null)
                return (false, "Bạn chưa theo dõi người này.");
            _context.FollowUsers.Remove(follow);
            await _context.SaveChangesAsync();
            return (true, "Bỏ theo dõi người dùng thành công.");
        }

        public async Task<bool> IsFollowingUserAsync(int followerId, int followingId)
        {
            return await _context.FollowUsers.AnyAsync(f => f.FollowerID == followerId && f.FollowingID == followingId);
        }

        public async Task<int> CountUserFollowersAsync(int userId)
        {
            return await _context.FollowUsers.CountAsync(f => f.FollowingID == userId);
        }

        public async Task<int> CountUserFollowingsAsync(int userId)
        {
            return await _context.FollowUsers.CountAsync(f => f.FollowerID == userId);
        }

        public async Task<List<UserFollowItemViewModel>> GetFollowingUsersAsync(int userId)
        {
            var users = await (from f in _context.FollowUsers
                               join u in _context.Users on f.FollowingID equals u.UserID
                               where f.FollowerID == userId
                               select new UserFollowItemViewModel
                               {
                                   UserID = u.UserID,
                                   UserName = u.DisplayName ?? u.Email,
                                   AvatarUrl = u.Avatar,
                                   ShortBio = u.Bio
                               }).ToListAsync();
            foreach (var user in users)
            {
                user.AvatarUrl = await _blobService.GetSafeImageUrlAsync(user.AvatarUrl ?? string.Empty);
            }
            return users;
        }

        public async Task<List<UserFollowItemViewModel>> GetFollowerUsersAsync(int userId)
        {
            var users = await (from f in _context.FollowUsers
                               join u in _context.Users on f.FollowerID equals u.UserID
                               where f.FollowingID == userId
                               select new UserFollowItemViewModel
                               {
                                   UserID = u.UserID,
                                   UserName = u.DisplayName ?? u.Email,
                                   AvatarUrl = u.Avatar,
                                   ShortBio = u.Bio
                               }).ToListAsync();
            foreach (var user in users)
            {
                user.AvatarUrl = await _blobService.GetSafeImageUrlAsync(user.AvatarUrl ?? string.Empty);
            }
            return users;
        }
    }
}
