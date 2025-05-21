using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Comment;
using PBL3.ViewModels.Comment;

namespace PBL3.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }
        //POST: /Comment/Post
        [HttpPost]
        public async Task<IActionResult> Post(CommentPostViewModel model, string type)
        {
            int? newCommentId = null;
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
            } else
            {
                var (isSuccess, message, commentId) = await _commentService.PostCommentAsync(model, type);

                if (isSuccess)
                {
                    TempData["SuccessMessage"] = message;
                    newCommentId = commentId;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }

            if (newCommentId != null)
            {
                TempData["ScrollToComment"] = $"comment-{newCommentId}";
            }

            if (model.ChapterID != null)
            {
                return RedirectToAction("ReadChapter", "Chapter", new { id = model.ChapterID });
            }
            else
            {
                return RedirectToAction("View", "Story", new { id = model.StoryID });
            }
        }

        public IActionResult GetCommentFormPartial(int chapterId, string userId, int? parentCommentId, string replyingToUsername, string formId, string type)
        {
            var viewModel = new PBL3.ViewModels.Chapter.CommentFormViewModel
            {
                ChapterID = chapterId,
                UserID = userId,
                ParentCommentID = parentCommentId,
                ReplyingToUsername = replyingToUsername,
                FormId = formId,
                Type = type
            };

            return PartialView("_CommentFormPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int commentId, int storyId, int? chapterId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "Không xác định được người dùng.";
                if (chapterId != null)
                    return RedirectToAction("ReadChapter", "Chapter", new { id = chapterId });
                else
                    return RedirectToAction("View", "Story", new { id = storyId });
            }
            int userId = int.Parse(userIdClaim.Value);
            var (isSuccess, message) = await _commentService.DeleteCommentAsync(commentId, userId);
            if (isSuccess)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }
            if (chapterId != null)
            {
                return RedirectToAction("ReadChapter", "Chapter", new { id = chapterId });
            }
            else
            {
                return RedirectToAction("View", "Story", new { id = storyId });
            }
        }
    }
}
