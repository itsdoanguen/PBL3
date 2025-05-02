using PBL3.Data;
using PBL3.ViewModels;
using PBL3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PBL3.ViewModels.UserProfile;

namespace PBL3.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public UserService(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
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
            return stories;
        }


        public async Task<UserProfileViewModel> GetUserProfile(int userId)
        {
            var userInfo = await _context.Users.FindAsync(userId);
            if(userInfo == null)
            {
                return null;
            }

            var stories = await GetUserStoryCard(userId);

            //Tách Url của avatar và banner
            string avatarBlobName = ExtractBlobName(userInfo.Avatar);
            string bannerBlobName = ExtractBlobName(userInfo.Banner);

            //Kiểm tra nếu có avatar hoặc banner thì lấy URL của blob, còn không thì trả về null
            var avatarUrl = string.IsNullOrEmpty(avatarBlobName) ? null : await _blobService.GetBlobSasUrlAsync(avatarBlobName);
            var bannerUrl = string.IsNullOrEmpty(bannerBlobName) ? null : await _blobService.GetBlobSasUrlAsync(bannerBlobName);

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

        //Hàm tách tên blob từ URL
        private string ExtractBlobName(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            //Vì URL có định dạng là "https://<storageName>.blob.core.windows.net/<containerName>/<blobName>" 
            //Dùng hàm IndexOf để tìm vị trí của tên container
            //Sau đó lấy phần còn lại của URL từ vị trí của container đến hết
            string containerName = "pbl3container/";
            int index = url.IndexOf(containerName);

            if (index != -1)
            {
                return url.Substring(index + containerName.Length);
            }

            return url;
        }
    }
}
