using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface IReportService
    {
        Task<ReportViewModel> GetReportDataAsync(string reportType, string dateRange);
        Task<byte[]> ExportToCsvAsync(string reportType, string dateRange);
    }
}