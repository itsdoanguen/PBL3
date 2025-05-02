using PBL3.ViewModels.Story;


namespace PBL3.Service
{
    public interface IStoryService
    {
<<<<<<< HEAD
        Task<int?> CreateStoryAsync(StoryCreateViewModel model, int authorID);
        Task DeleteStoryAsync(int storyID);
        //Task<StoryEditViewModel> getStoryEditDetailsAsync(int storyID, int authorID);
=======
        Task<(bool isSuccess, string errorMessage, int? storyID)> CreateStoryAsync(StoryCreateViewModel model, int authorID);
        Task<(bool isSuccess, string errorMessage)> DeleteStoryAsync(int storyID, int currentUserID);
        Task<StoryEditViewModel> GetStoryDetailForEditAsync(int storyID, int currentAuthorID);
>>>>>>> 837ffd5ef3e887057fba17ebf25cc423a0dce6b8
    }
}
