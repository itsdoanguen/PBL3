using System;
using System.Collections.Generic;

namespace PBL3.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        // Thống kê tổng quan
        public int TotalUsers { get; set; }
        public int TotalStories { get; set; }
        public int TotalChapters { get; set; }
        public int TotalComments { get; set; }
        public int TotalReports { get; set; }

        // Thống kê theo vai trò
        public int TotalAdmins { get; set; }
        public int TotalModerators { get; set; }
        public int TotalNormalUsers { get; set; }

        // Thống kê truyện theo trạng thái
        public int ActiveStories { get; set; }
        public int LockedStories { get; set; }
        public int ReviewPendingStories { get; set; }
        public int CompletedStories { get; set; }

        // Thống kê user mới trong 7 ngày gần nhất
        public int NewUsersLast7Days { get; set; }
    }

    public class AdminReportViewModel
    {
        // Thống kê user mới
        public int TotalNewUsers { get; set; }
        public List<UserPerDay> NewUsersPerDay { get; set; } = new List<UserPerDay>();

        // Thống kê truyện mới
        public int TotalNewStories { get; set; }
        public List<StoryPerDay> NewStoriesPerDay { get; set; } = new List<StoryPerDay>();

        // Thống kê báo cáo (report) - tách riêng khỏi notification thường
        public int TotalReports { get; set; }
        public List<ReportPerDay> ReportsPerDay { get; set; } = new List<ReportPerDay>();
        public List<ReportTypeCount> ReportTypeCounts { get; set; } = new List<ReportTypeCount>();

        // Lọc theo thể loại/tag
        public List<GenreTagItem> AvailableTags { get; set; } = new List<GenreTagItem>();
        public int? SelectedTagId { get; set; }
        public int NewStoriesByTag { get; set; }

        // Khoảng thời gian lọc
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UserPerDay
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
    public class StoryPerDay
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
    public class GenreTagItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    public class ReportPerDay
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
    public class ReportTypeCount
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
