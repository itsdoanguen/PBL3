using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Follow;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PBL3.Controllers
{
    [Authorize]
    public class FollowController : Controller
    {
        private readonly IFollowService _followService;
        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStoryFollow(int storyId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            bool isFollowing = await _followService.IsFollowingStoryAsync(userId, storyId);
            (bool isSuccess, string message) result;
            if (isFollowing)
            {
                result = await _followService.UnfollowStoryAsync(userId, storyId);
            }
            else
            {
                result = await _followService.FollowStoryAsync(userId, storyId);
            }

            // Có thể trả về JSON nếu dùng AJAX, hoặc redirect kèm TempData nếu dùng form thường
            TempData["FollowMessage"] = result.message;
            return RedirectToAction("View", "Story", new { id = storyId });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserFollow(int followingId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();
            int followerId = int.Parse(userIdClaim.Value);

            if (followerId == followingId)
            {
                TempData["FollowMessage"] = "Bạn không thể theo dõi chính mình.";
                return RedirectToAction("ViewProfile", "User", new { id = followingId });
            }

            bool isFollowing = await _followService.IsFollowingUserAsync(followerId, followingId);
            (bool isSuccess, string message) result;
            if (isFollowing)
            {
                result = await _followService.UnfollowUserAsync(followerId, followingId);
            }
            else
            {
                result = await _followService.FollowUserAsync(followerId, followingId);
            }

            TempData["FollowMessage"] = result.message;
            return RedirectToAction("ViewProfile", "User", new { id = followingId });
        }
    }
}
