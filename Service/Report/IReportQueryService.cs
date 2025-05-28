using System.Threading.Tasks;
using System.Collections.Generic;
using PBL3.ViewModels.Moderator;

namespace PBL3.Service.Report
{
    public interface IReportQueryService
    {
        Task<List<ReportViewModel>> GetReportsCreatedByUserAsync(int userId);
        Task<List<ReportViewModel>> GetReportsReceivedByUserAsync(int userId);
    }
}
