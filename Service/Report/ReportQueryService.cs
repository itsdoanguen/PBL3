using PBL3.ViewModels.Moderator;

namespace PBL3.Service.Report
{
    public class ReportQueryService : IReportQueryService
    {
        private readonly IReportService _reportService;
        public ReportQueryService(IReportService reportService)
        {
            _reportService = reportService;
        }
        public async Task<List<ReportViewModel>> GetReportsCreatedByUserAsync(int userId)
        {
            var reports = await _reportService.GetReportNotificationsAsync();
            return reports
                .Where(r => r.FromUserID == userId)
                .Select(r => new ReportViewModel
                {
                    ReportID = r.NotificationID,
                    ReportedUserID = r.UserID,
                    ReporterID = r.FromUserID,
                    StoryID = r.StoryID,
                    ChapterID = r.ChapterID,
                    CommentID = r.CommentID,
                    Reason = r.Message,
                    ReportType = r.Type,
                    CreatedAt = r.CreatedAt,
                    Status = r.IsRead
                })
                .ToList();
        }
        public async Task<List<ReportViewModel>> GetReportsReceivedByUserAsync(int userId)
        {
            var reports = await _reportService.GetReportNotificationsAsync();
            return reports
                .Where(r => r.UserID == userId)
                .Select(r => new ReportViewModel
                {
                    ReportID = r.NotificationID,
                    ReportedUserID = r.UserID,
                    ReporterID = r.FromUserID,
                    StoryID = r.StoryID,
                    ChapterID = r.ChapterID,
                    CommentID = r.CommentID,
                    Reason = r.Message,
                    ReportType = r.Type,
                    CreatedAt = r.CreatedAt,
                    Status = r.IsRead
                })
                .ToList();
        }
    }
}
