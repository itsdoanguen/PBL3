using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.Service.Like;

namespace PBL3.Controllers
{
    [Authorize]
    public class LikeChapterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILikeChapterService _likeChapterService;
        public LikeChapterController(ApplicationDbContext context, ILikeChapterService likeChapterService)
        {
            _context = context;
            _likeChapterService = likeChapterService;
        }

        // POST: LikeChapter/LikeChapter/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeChapter(int chapterId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            bool isLiked = await _likeChapterService.LikeChapterAsync(chapterId, currentUserId);

            if (isLiked)
            {
                TempData["Success"] = "You liked this chapter.";
            }
            else
            {
                TempData["Error"] = "You unliked this chapter.";
            }
            return RedirectToAction("ReadChapter", "Chapter", new
            {
                id = chapterId
            });
        }

        // GET: LikeChapter/LikedChapters
        [HttpGet]
        public async Task<IActionResult> LikedChapters()
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var likedChapters = await _likeChapterService.GetLikedChaptersByUserAsync(currentUserId);
            return View(likedChapters);
        }
    }
}
