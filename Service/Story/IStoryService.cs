using PBL3.ViewModels.Story;


namespace PBL3.Service.Story
{
    public interface IStoryService
    {
        Task<(bool isSuccess, string errorMessage, int? storyID)> CreateStoryAsync(StoryCreateViewModel model, int authorID);
        Task<(bool isSuccess, string errorMessage)> DeleteStoryAsync(int storyID, int currentUserID);
        Task<(bool isSuccess, string errorMessage, int storyID)> UpdateStoryStatusAsync(int storyID, int currentUserID, string newStatus);
        Task<(bool isSuccess, string errorMessage)> UpdateStoryAsync(StoryEditViewModel model, int currentUserID);
        Task<(bool isSuccess, string errorMessage)> LockStoryAsync(int storyID, string message, int moderatorId);
        Task<(bool isSuccess, string errorMessage)> UnlockStoryAsync(int storyID, bool isAccepted, string message, int moderatorId);
        Task<(bool isSuccess, string errorMessage)> PendingReviewAsync(int storyID, int currentUserId);
    }
}
