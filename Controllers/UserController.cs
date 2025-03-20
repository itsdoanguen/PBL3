using System.Security.Claims;
using Azure.Core.Pipeline;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.ViewModels;

namespace PBL3.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        //GET: User/Index
        public IActionResult Index()
        {
            return View();
        }
        //GET: User/MyProfile
        public IActionResult MyProfile()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            UserProfileViewModel profile = GetUserProfile(id);
         
            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }
        //POST: User/EditProfile



        //GET: User/ViewProfile
        public IActionResult ViewProfile(int id)
        {
            UserProfileViewModel profile = new UserProfileViewModel();

            profile = GetUserProfile(id);

            if (profile == null)
            {
                return NotFound();
            }
            return View(profile);
        }

        private UserProfileViewModel GetUserProfile(int id)
        {
            var userInfo = _context.Users.Find(id);
            if (userInfo == null)
            {
                return null; 
            }

            var stories = _context.Stories
                .Where(s => s.AuthorID == id)
                .Select(s => new UserStoryCardViewModel
                {
                    StoryID = s.StoryID,
                    Title = s.Title,
                    Cover = s.CoverImage,
                    LastUpdated = s.UpdatedAt,
                    Status = s.Status,
                    TotalChapters = _context.Chapters.Count(c => c.StoryID == s.StoryID),
                })
                .ToList();

            var profile = new UserProfileViewModel
            {
                DisplayName = userInfo.DisplayName,
                Email = userInfo.Email,
                Avatar = userInfo.Avatar,
                Banner = userInfo.Banner,
                Bio = userInfo.Bio,
                CreatedAt = userInfo.CreatedAt,
                DateOfBirth = userInfo.DateOfBirth,
                Role = userInfo.Role,
                Status = userInfo.Status,
                TotalUploadedStories = stories.Count,
                Stories = stories,
                TotalFollowers = _context.FollowUsers.Count(f => f.FollowingID == id),
                TotalFollowings = _context.FollowUsers.Count(f => f.FollowerID == id),
                TotalComments = _context.Comments.Count(c => c.UserID == id)
            };

            return profile;
        }
    }
}
