using Microsoft.EntityFrameworkCore;
using PBL3.Data;
using PBL3.Models;
using PBL3.ViewModels.Chapter;
using PBL3.ViewModels.Comment;

namespace PBL3.Service.Comment
{
    public class CommentMappingService : ICommentMappingService
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public CommentMappingService(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }
        // Lấy comment gốc và reply trực tiếp (2 tầng đầu)
        public async Task<List<CommentTreeViewModel>> GetRootAndFirstLevelRepliesAsync(string type, int id)
        {
            var comments = await GetCommentsByTypeAsync(type, id);
            var users = await GetUserDictionaryAsync();
            var commentDict = comments.ToDictionary(c => c.CommentID, c => c);
            var rootComments = comments.Where(c => c.ParentCommentID == null).ToList();
            var rootIds = rootComments.Select(c => c.CommentID).ToList();
            var replies = comments.Where(c => c.ParentCommentID != null && rootIds.Contains(c.ParentCommentID.Value)).ToList();
            var dict = rootComments.ToDictionary(
                c => c.CommentID,
                c => new CommentTreeViewModel { Comment = MapToFlatViewModelAsync(c, commentDict, users).Result, Replies = new List<CommentTreeViewModel>() }
            );
            foreach (var reply in replies)
            {
                if (reply.ParentCommentID != null && dict.TryGetValue(reply.ParentCommentID.Value, out var parent))
                {
                    parent.Replies.Add(new CommentTreeViewModel
                    {
                        Comment = MapToFlatViewModelAsync(reply, commentDict, users).Result,
                        Replies = new List<CommentTreeViewModel>()
                    });
                }
            }
            return dict.Values.ToList();
        }
        // Lấy replies theo parentId (lazy load)
        public async Task<List<CommentTreeViewModel>> GetRepliesAsync(string type, int id, int parentCommentId)
        {
            var comments = await GetCommentsByTypeAsync(type, id);
            var users = await GetUserDictionaryAsync();
            var commentDict = comments.ToDictionary(c => c.CommentID, c => c);
            var replies = comments.Where(c => c.ParentCommentID == parentCommentId).ToList();
            var result = new List<CommentTreeViewModel>();
            foreach (var reply in replies)
            {
                result.Add(new CommentTreeViewModel
                {
                    Comment = await MapToFlatViewModelAsync(reply, commentDict, users),
                    Replies = new List<CommentTreeViewModel>()
                });
            }
            return result;
        }
        private async Task<List<CommentModel>> GetCommentsByTypeAsync(string type, int id)
        {
            return type switch
            {
                "story" => await _context.Comments.Where(c => c.StoryID == id).OrderByDescending(c => c.CreatedAt).ToListAsync(),
                "chapter" => await _context.Comments.Where(c => c.ChapterID == id).OrderByDescending(c => c.CreatedAt).ToListAsync(),
                _ => throw new ArgumentException("Loại comment không hợp lệ")
            };
        }
        private async Task<Dictionary<int, (string DisplayName, string Avatar)>> GetUserDictionaryAsync()
        {
            return await _context.Users.ToDictionaryAsync(u => u.UserID, u => (u.DisplayName, u.Avatar));
        }
        private async Task<CommentPostViewModel> MapToFlatViewModelAsync(CommentModel comment, Dictionary<int, CommentModel> commentDict, Dictionary<int, (string DisplayName, string Avatar)> userDict)
        {
            string? parentUserName = null;
            if (comment.ParentCommentID.HasValue && commentDict.TryGetValue(comment.ParentCommentID.Value, out var parentComment) && userDict.TryGetValue(parentComment.UserID, out var parentUser))
            {
                parentUserName = parentUser.DisplayName;
            }
            var user = userDict.GetValueOrDefault(comment.UserID);
            int repliesCount = commentDict.Values.Count(c => c.ParentCommentID == comment.CommentID);
            return new CommentPostViewModel
            {
                CommentID = comment.CommentID,
                StoryID = comment.StoryID,
                ChapterID = comment.ChapterID,
                ParentCommentID = comment.ParentCommentID,
                ParentUserName = parentUserName,
                UserID = comment.UserID,
                UserName = user.DisplayName,
                UserAvatar = await _blobService.GetSafeImageUrlAsync(user.Avatar),
                Content = comment.Content,
                isDeleted = comment.isDeleted,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                RepliesCount = repliesCount
            };
        }
    }
}
