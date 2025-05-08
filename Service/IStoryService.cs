using PBL3.ViewModels.Story;


namespace PBL3.Service
{
    public interface IStoryService
    {
        Task<(bool isSuccess, string errorMessage, int? storyID)> CreateStoryAsync(StoryCreateViewModel model, int authorID);
        Task<(bool isSuccess, string errorMessage)> DeleteStoryAsync(int storyID, int currentUserID);
        Task<StoryEditViewModel> GetStoryDetailForEditAsync(int storyID, int currentAuthorID);
        Task<(bool isSuccess, string errorMessage, int storyID)> UpdateStoryStatusAsync(int storyID, int currentUserID, string newStatus);
        Task<(bool isSuccess, string errorMessage)> UpdateStoryAsync(StoryEditViewModel model, int currentUserID);
        Task<StoryDetailViewModel> GetStoryDetailAsync(int storyID, int currentUserID);
    }
}
