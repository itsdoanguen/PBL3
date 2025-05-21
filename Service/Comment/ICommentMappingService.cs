using PBL3.ViewModels.Chapter;
using PBL3.ViewModels.Comment;

namespace PBL3.Service.Comment
{
    public interface ICommentMappingService
    {
        Task<List<CommentTreeViewModel>> GetRootAndFirstLevelRepliesAsync(string type, int id);
        Task<List<CommentTreeViewModel>> GetRepliesAsync(string type, int id, int parentCommentId);
    }
}
