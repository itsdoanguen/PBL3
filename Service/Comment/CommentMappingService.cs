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
        public async Task<List<CommentPostViewModel>> GetFlatCommentsAsync(string type, int id)
        {
            var comments = await GetCommentsByTypeAsync(type, id);
            var users = await GetUserDictionaryAsync();
            var commentDict = comments.ToDictionary(c => c.CommentID, c => c);

            var result = new List<CommentPostViewModel>();

            foreach (var comment in comments)
            {
                var viewModel = await MapToFlatViewModelAsync(comment, commentDict, users);
                result.Add(viewModel);
            }

            return result;
        }
        private async Task<List<CommentModel>> GetCommentsByTypeAsync(string type, int id)
        {
            return type switch
            {
                "story" => await _context.Comments
                              .Where(c => c.StoryID == id)
                              .OrderBy(c => c.CreatedAt)
                              .ToListAsync(),

                "chapter" => await _context.Comments
                                .Where(c => c.ChapterID == id)
                                .OrderBy(c => c.CreatedAt)
                                .ToListAsync(),

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

            if (comment.ParentCommentID.HasValue &&
                commentDict.TryGetValue(comment.ParentCommentID.Value, out var parentComment) &&
                userDict.TryGetValue(parentComment.UserID, out var parentUser))
            {
                parentUserName = parentUser.DisplayName;
            }

            var user = userDict.GetValueOrDefault(comment.UserID);

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
                UpdatedAt = comment.UpdatedAt
            };
        }
        public async Task<List<CommentTreeViewModel>> GetCommentTreeAsync(string type, int id)
        {
            var flatComments = await GetFlatCommentsAsync(type, id);
            var lookup = flatComments.ToLookup(c => c.ParentCommentID);

            List<CommentTreeViewModel> BuildTree(int? parentId)
            {
                return lookup[parentId]
                    .Select(c => new CommentTreeViewModel
                    {
                        Comment = c,
                        Replies = BuildTree(c.CommentID)
                    }).ToList();
            }

            return BuildTree(null); 
        }

    }
}
