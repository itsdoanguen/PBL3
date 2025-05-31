using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3.Models;
using PBL3.Service.Notification;
using PBL3.Service.Report;

namespace PBL3.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly INotificationService _notificationService;
        public ReportController(IReportService reportService, INotificationService notificationService)
        {
            _reportService = reportService;
            _notificationService = notificationService;
        }
        private async Task<IActionResult> HandleReportAsync(
            int targetId,
            string message,
            NotificationModel.NotificationType reportType,
            string? returnUrl = null,
            bool checkSelfReport = false)
        {
            try
            {
                var fromUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (targetId <= 0 || string.IsNullOrWhiteSpace(message))
                {
                    TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                    return RedirectToAction("Error", "Error");
                }

                if (fromUserId <= 0)
                {
                    TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                    return RedirectToAction("Login", "Authentication", new { returnUrl = returnUrl ?? Url.Action("Index", "User") });
                }

                if (checkSelfReport && targetId == fromUserId)
                {
                    TempData["ErrorMessage"] = "Bạn không thể tự báo cáo chính mình.";
                    return RedirectToAction("Error", "Error");
                }

                switch (reportType)
                {
                    case NotificationModel.NotificationType.ReportUser:
                        await _reportService.InitReportUserNotificationAsync(targetId, fromUserId, message);
                        break;
                    case NotificationModel.NotificationType.ReportComment:
                        await _reportService.InitReportCommentNotificationAsync(targetId, fromUserId, message);
                        break;
                    case NotificationModel.NotificationType.ReportChapter:
                        await _reportService.InitReportChapterNotificationAsync(targetId, fromUserId, message);
                        break;
                    case NotificationModel.NotificationType.ReportStory:
                        await _reportService.InitReportStoryNotificationAsync(targetId, fromUserId, message);
                        break;
                    default:
                        TempData["ErrorMessage"] = "Loại báo cáo không hợp lệ.";
                        return RedirectToAction("Error", "Error");
                }

                TempData["SuccessMessage"] = "Đã gửi báo cáo thành công.";

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "User");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý báo cáo.";
                return RedirectToAction("Error", "Error");
            }
        }

        // POST: /Report/ReportUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportUser(int reportedUserId, string message, string? returnUrl = null)
        {
            return await HandleReportAsync(reportedUserId, message, NotificationModel.NotificationType.ReportUser, returnUrl, true);
        }

        // POST: /Report/ReportComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportComment(int commentId, string message, string? returnUrl = null)
        {
            return await HandleReportAsync(commentId, message, NotificationModel.NotificationType.ReportComment, returnUrl);
        }

        // POST: /Report/ReportChapter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportChapter(int chapterId, string message, string? returnUrl = null)
        {
            return await HandleReportAsync(chapterId, message, NotificationModel.NotificationType.ReportChapter, returnUrl);
        }

        // POST: /Report/ReportStory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportStory(int storyId, string message, string? returnUrl = null)
        {
            return await HandleReportAsync(storyId, message, NotificationModel.NotificationType.ReportStory, returnUrl);
        }

        // GET: /Report/Index
        [Authorize(Roles = "Moderator,Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reports = await _reportService.GetReportNotificationsAsync();
            return View(reports);
        }
        //POST: /Report/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return RedirectToAction("Index");
        }
        //POST: /Report/DeleteNotification  
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator,Admin")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var noti = await _notificationService.GetNotificationByIdAsync(notificationId);
            if (noti == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy báo cáo để xóa!";
                return RedirectToAction("Index");
            }
            if (!noti.IsRead)
            {
                TempData["ErrorMessage"] = "Chỉ được xóa báo cáo đã xử lý!";
                return RedirectToAction("Index");
            }
            var (isSuccess, message) = await _notificationService.DeleteNotificationAsync(notificationId);
            TempData[isSuccess ? "SuccessMessage" : "ErrorMessage"] = message;
            return RedirectToAction("Index");
        }
    }
}
