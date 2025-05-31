using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Chapter;
using PBL3.ViewModels.Comment;
using PBL3.Service.Notification;

namespace PBL3.Service.Comment
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly ICommentMappingService _commentMappingService;
        private readonly INotificationService _notificationService;
        public CommentService(ApplicationDbContext context, BlobService blobService, ICommentMappingService commentMappingService, INotificationService notificationService)
        {
            _context = context;
            _blobService = blobService;
            _commentMappingService = commentMappingService;
            _notificationService = notificationService;
        }
        public async Task<(bool isSuccess, string message, int? commentId)> PostCommentAsync(CommentPostViewModel model, string type)
        {
            int? newCommentId = null;
            var (isValid, message) = await CommentValidateAsync(model, type);
            if (!isValid)
            {
                return (false, message, null);
            }

            switch (type)
            {
                case "chapter":
                    var (postChapterSucess, postChapterMessage, postChapterCommentId) = await PostCommentOnChapterAsync(model);
                    if (!postChapterSucess)
                    {
                        return (false, postChapterMessage, null);
                    }
                    newCommentId = postChapterCommentId;
                    if (newCommentId.HasValue)
                    {
                        await _notificationService.InitNewCommentNotificationAsync(model.StoryID ?? 0, newCommentId.Value, model.UserID);
                    }
                    break;
                case "story":
                    var (postStorySucess, postStoryMessage, postStoryCommentId) = await PostCommentOnStoryAsync(model);
                    if (!postStorySucess)
                    {
                        return (false, postStoryMessage, null);
                    }
                    newCommentId = postStoryCommentId;
                    if (newCommentId.HasValue)
                    {
                        await _notificationService.InitNewCommentNotificationAsync(model.StoryID ?? 0, newCommentId.Value, model.UserID);
                    }
                    break;
                case "reply":
                    var (postReplySucess, postReplyMessage, postReplyCommentId) = await PostReplyAsync(model);
                    if (!postReplySucess)
                    {
                        return (false, postReplyMessage, null);
                    }
                    newCommentId = postReplyCommentId;
                    if (newCommentId.HasValue)
                    {
                        await _notificationService.InitNewReplyCommentNotificationAsync(newCommentId.Value, model.UserID);
                    }
                    break;
                default:
                    return (false, "Loại bình luận không hợp lệ", null);
            }

            return (true, "Bình luận thành công", newCommentId);
        }

        private async Task<(bool isSuccess, string message, int? commentId)> PostCommentOnChapterAsync(CommentPostViewModel model)
        {
            var comment = new CommentModel
            {
                UserID = model.UserID,
                ChapterID = model.ChapterID,
                StoryID = null,
                ParentCommentID = null,
                Content = model.Content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                isDeleted = false
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return (true, "Bình luận trên chương thành công", comment.CommentID);
        }

        private async Task<(bool isSuccess, string message, int? commentId)> PostCommentOnStoryAsync(CommentPostViewModel model)
        {
            var comment = new CommentModel
            {
                UserID = model.UserID,
                StoryID = model.StoryID,
                ChapterID = null,
                ParentCommentID = null,
                Content = model.Content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                isDeleted = false
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return (true, "Bình luận trên story thành công", comment.CommentID);
        }

        private async Task<(bool isSuccess, string message, int? commentId)> PostReplyAsync(CommentPostViewModel model)
        {
            var parentComment = await _context.Comments.FindAsync(model.ParentCommentID);
            if (parentComment == null)
            {
                return (false, "Không tìm thấy comment đang reply tới", null);
            }

            var chapterId = parentComment.ChapterID;
            var storyId = parentComment.StoryID;


            var comment = new CommentModel
            {
                UserID = model.UserID,
                ChapterID = chapterId,
                StoryID = storyId,
                ParentCommentID = model.ParentCommentID,
                Content = model.Content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                isDeleted = false
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return (true, "Trả lời thành công", comment.CommentID);
        }

        public async Task<(bool isSuccess, string message)> CommentValidateAsync(CommentPostViewModel model, string type)
        {
            if (string.IsNullOrWhiteSpace(model.Content))
            {
                return (false, "Nội dung của bình luận không được để trống");
            }
            if (model.UserID == 0)
            {
                return (false, "Người dùng không hợp lệ");
            }
            switch (type)
            {
                case "chapter":
                    if (model.ChapterID == null)
                    {
                        return (false, "Chương không hợp lệ");
                    }
                    break;
                case "story":
                    if (model.StoryID == null)
                    {
                        return (false, "Truyện không hợp lệ");
                    }
                    break;
                case "reply":
                    if (model.ParentCommentID == null)
                    {
                        return (false, "Bình luận không hợp lệ");
                    }
                    break;
                default:
                    return (false, "Bạn muốn bình luận cho cái gì?");
            }
            return (true, "Comment valid");
        }

        public async Task<List<CommentTreeViewModel>> GetRootAndFirstLevelRepliesAsync(string type, int id)
        {
            return await _commentMappingService.GetRootAndFirstLevelRepliesAsync(type, id);
        }

        // Lấy replies theo parentId 
        public async Task<List<CommentTreeViewModel>> GetRepliesAsync(string type, int id, int parentCommentId)
        {
            return await _commentMappingService.GetRepliesAsync(type, id, parentCommentId);
        }

        public async Task<(bool isSuccess, string message)> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null || comment.isDeleted)
            {
                return (false, "Không tìm thấy bình luận hoặc đã bị xóa");
            }
            var userRole = _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => u.Role)
                .FirstOrDefault();
            if (comment.UserID != userId && userRole == UserModel.UserRole.User)
            {
                return (false, "Bạn không có quyền xóa bình luận này");
            }
            comment.isDeleted = true;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return (true, "Xóa bình luận thành công");
        }

        public Task<List<CommentTreeViewModel>> GetCommentsAsync(string type, int id)
        {
            return GetRootAndFirstLevelRepliesAsync(type, id);
        }
    }
}
