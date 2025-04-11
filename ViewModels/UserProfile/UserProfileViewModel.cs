using PBL3.Models;

namespace PBL3.ViewModels.UserProfile
{
    public class UserProfileViewModel
    {
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Banner { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public UserModel.UserRole Role { get; set; }
        public UserModel.UserStatus Status { get; set; }
        public UserModel.UserGender? Gender { get; set; }
        public int TotalUploadedStories { get; set; }
        public int TotalFollowers { get; set; }
        public int TotalFollowings { get; set; }
        public int TotalComments { get; set; }

        public ICollection<UserStoryCardViewModel> Stories { get; set; } = new List<UserStoryCardViewModel>();
    }
}
