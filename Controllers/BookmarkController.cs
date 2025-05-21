using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Bookmark;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PBL3.Controllers
{
    [Authorize]
    public class BookmarkController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        public BookmarkController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int chapterId)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                TempData["BookmarkMessage"] = "Không xác định được người dùng.";
                return RedirectToAction("ReadChapter", "Chapter", new { id = chapterId });
            }
            int userId = int.Parse(userIdClaim.Value);
            var result = await _bookmarkService.ToggleBookmarkAsync(userId, chapterId);
            TempData["BookmarkMessage"] = result.Message;
            return RedirectToAction("ReadChapter", "Chapter", new { id = chapterId });
        }
    }
}
