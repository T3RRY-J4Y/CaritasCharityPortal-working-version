using CaritasCharityPortal.Models;
using CaritasCharityPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaritasCharityPortal.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var isAdmin = User.IsInRole("Admin");
            DashboardViewModel model;

            if (isAdmin)
            {
                model = await _dashboardService.GetAdminDashboardAsync();
            }
            else
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrEmpty(userEmail)) userEmail = "";
                model = await _dashboardService.GetUserDashboardAsync(userEmail);
            }

            ViewBag.IsAdmin = isAdmin;
            return View(model);
        }
    }
}