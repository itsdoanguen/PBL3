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
        //public IActionResult Index()
        //{
        //    return View();
        //}

        // POST: LikeChapter/LikeChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeChapter(int chapterId)
        {
            string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            int userId = int.Parse(userIdStr);
            (bool liked, int likeCount) = await _likeChapterService.LikeChapterAsync(chapterId, userId);

            return Json(new { success = true, liked, likeCount });
        }
    }
}
