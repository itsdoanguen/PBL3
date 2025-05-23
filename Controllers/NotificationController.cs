using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Service.Notification;
using System.Threading.Tasks;

namespace PBL3.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        // GET: Notification/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return RedirectToAction("Index");
        }
    }
}
