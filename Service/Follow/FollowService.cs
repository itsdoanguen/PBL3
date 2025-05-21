using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;

namespace PBL3.Service.Follow
{
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;
        public FollowService(ApplicationDbContext context)
        {
            _context = context;
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

        // ...implement User follow nếu cần...
    }
}
