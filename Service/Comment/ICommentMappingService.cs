using PBL3.ViewModels.Chapter;
using PBL3.ViewModels.Comment;

namespace PBL3.Service.Comment
{
    public interface ICommentMappingService
    {
        Task<List<CommentPostViewModel>> GetFlatCommentsAsync(string type, int id);
        Task<List<CommentTreeViewModel>> GetCommentTreeAsync(string type, int id);
    }
}
