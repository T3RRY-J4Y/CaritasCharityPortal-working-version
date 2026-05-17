using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetAdminDashboardAsync();
        Task<DashboardViewModel> GetUserDashboardAsync(string userEmail);
    }
}