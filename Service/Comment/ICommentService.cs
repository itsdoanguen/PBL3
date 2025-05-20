using PBL3.ViewModels.Chapter;
using PBL3.ViewModels.Comment;

namespace PBL3.Service.Comment
{
    public interface ICommentService
    {
        Task<(bool isSuccess, string message, int? commentId)> PostCommentAsync(CommentPostViewModel model, string type);
        Task<(bool isSuccess, string message)> CommentValidateAsync(CommentPostViewModel model, string type);
        Task<List<CommentTreeViewModel>> GetCommentsAsync(string type, int id);
        Task<(bool isSuccess, string message)> DeleteCommentAsync(int commentId, int userId);
    }
}
