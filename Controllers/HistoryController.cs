using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.History;

namespace PBL3.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly IHistoryService _historyService;
        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }
        // GET: HistoryController
        public async Task<ActionResult> Index()
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var historyItems = await _historyService.GetUserHistoryAsync(userId);
            if (historyItems == null || !historyItems.Any())
            {
                TempData["Message"] = "You haven't read any stories yet.";
                return View();
            }
            return View(historyItems);
        }

        // POST: History/DeleteHistory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHistory(int historyId)
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            // Lấy history cần xóa
            var histories = await _historyService.GetUserHistoryAsync(userId);
            var history = histories.FirstOrDefault(h => h.HistoryID == historyId);
            if (history == null)
            {
                TempData["ErrorMessage"] = "History not found.";
                return RedirectToAction("Index");
            }
            // Xóa lịch sử đọc (theo storyId)
            await _historyService.DeleteHistoryAsync(userId, history.StoryID);
            TempData["SuccessMessage"] = "Reading history deleted successfully.";
            return RedirectToAction("Index", "History");
        }

    }
}
