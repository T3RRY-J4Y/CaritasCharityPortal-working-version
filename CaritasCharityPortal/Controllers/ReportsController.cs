using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CaritasCharityPortal.Services;

namespace CaritasCharityPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(string reportType = "Monthly Impact", string dateRange = "This Month")
        {
            var model = await _reportService.GetReportDataAsync(reportType, dateRange);
            ViewBag.SelectedReportType = reportType;
            ViewBag.SelectedDateRange = dateRange;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Export(string reportType, string dateRange, string format)
        {
            var csvData = await _reportService.ExportToCsvAsync(reportType, dateRange);
            var fileName = $"{reportType}_{dateRange}_{DateTime.Now:yyyyMMdd}.csv";
            return File(csvData, "text/csv", fileName);
        }
    }
}