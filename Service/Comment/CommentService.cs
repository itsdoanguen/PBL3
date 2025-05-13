using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Chapter;
using PBL3.ViewModels.Comment;

namespace PBL3.Service.Comment
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        private readonly ICommentMappingService _commentMappingService;
        public CommentService(ApplicationDbContext context, BlobService blobService, ICommentMappingService commentMappingService)
        {
            _context = context;
            _blobService = blobService;
            _commentMappingService = commentMappingService;
        }
        public async Task<(bool isSuccess, string message)> PostCommentAsync(CommentPostViewModel model, string type)
        {
            var (isValid, message) = await CommentValidateAsync(model, type);
            if (!isValid)
            {
                return (false, message);
            }

            switch (type)
            {
                case "chapter":
                    var (postChapterSucess, postChapterMessage) = await PostCommentOnChapterAsync(model);
                    if (!postChapterSucess)
                    {
                        return (false, postChapterMessage);
                    }
                    break;
                case "story":
                    var (postStorySucess, postStoryMessage) = await PostCommentOnStoryAsync(model);
                    if (!postStorySucess)
                    {
                        return (false, postStoryMessage);
                    }
                    break;
                case "reply":
                    var (postReplySucess, postReplyMessage) = await PostReplyAsync(model);
                    if (!postReplySucess)
                    {
                        return (false, postReplyMessage);
                    }
                    break;
                default:
                    return (false, "Loại bình luận không hợp lệ");
            }

            return (true, "Bình luận thành công");
        }

        private async Task<(bool isSuccess, string message)> PostCommentOnChapterAsync(CommentPostViewModel model)
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
            return (true, "Bình luận trên chương thành công");
        }

        private async Task<(bool isSuccess, string message)> PostCommentOnStoryAsync(CommentPostViewModel model)
        {
            var comment = new CommentModel
            {
                UserID = model.UserID,
                StoryID = model.StoryID,
                ChapterID = null,
                ParentCommentID = null,
                Content = model.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                isDeleted = false
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return (true, "Bình luận trên story thành công");
        }

        private async Task<(bool isSuccess, string message)> PostReplyAsync(CommentPostViewModel model)
        {
            var parentComment = await _context.Comments.FindAsync(model.ParentCommentID);
            if (parentComment == null)
            {
                return (false, "Không tìm thấy comment đang reply tới");
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                isDeleted = false
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return (true, "Trả lời thành công");
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

        public async Task<List<CommentTreeViewModel>> GetCommentsAsync(string type, int id)
        {
            var comments = await _commentMappingService.GetCommentTreeAsync(type, id);
            return comments;
        }

    }
}
