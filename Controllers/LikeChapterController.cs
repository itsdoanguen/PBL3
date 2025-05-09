using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.Models;
using PBL3.Service;

namespace PBL3.Controllers
{
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
                TempData["Success"] = "Bạn đã thích chương này.";
            }
            else
            {
                TempData["Error"] = "Bạn đã bỏ thích chương này.";
            }
            return RedirectToAction("ReadChapter", "Chapter", new
            {
                id = chapterId
            });
        }
    }
}
