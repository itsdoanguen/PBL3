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
            var historyItems = _historyService.GetUserHistoryAsync(userId).Result;
            if (historyItems == null || !historyItems.Any())
            {
                TempData["Message"] = "Bạn chưa đọc truyện nào.";
                return View();
            }
            return View(historyItems);
        }

    }
}
