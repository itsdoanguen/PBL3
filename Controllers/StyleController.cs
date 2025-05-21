using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Data;
using PBL3.Service.Style;
using PBL3.ViewModels.Chapter;

namespace PBL3.Controllers
{
    [Authorize]
    public class StyleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStyleService _styleService;
        public StyleController(ApplicationDbContext context, IStyleService styleService)
        {
            _context = context;
            _styleService = styleService;
        }
        //GET: Style/UpdateStyle
        [HttpGet]
        public async Task<IActionResult> UpdateStyle(int userId)
        {
            var style = await _styleService.GetStyleByUserIdAsync(userId);
            return View(style);
        }
        //POST: Style/UpdateStyle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStyle(StyleViewModel model, int ReturnChapterID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Dữ liệu không hợp lệ");
            }

            var updatedStyle = await _styleService.UpdateStyleAsync(model);

            return RedirectToAction("ReadChapter","Chapter",new {id = ReturnChapterID });
        }
    }
}
