using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Comment;
using PBL3.ViewModels.Comment;

namespace PBL3.Controllers
{
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
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
            } else
            {
                var (isSuccess, message) = await _commentService.PostCommentAsync(model, type);

                if (isSuccess)
                {
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
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
    }
}
