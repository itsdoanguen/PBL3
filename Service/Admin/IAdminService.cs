using PBL3.ViewModels.Admin;

namespace PBL3.Service.Admin
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();
        Task<AdminReportViewModel> GetReportStatsAsync(DateTime? from = null, DateTime? to = null, int? tagId = null);
        Task<AdminManageSystemViewModel> GetManageSystemStatsAsync();
    }
}
