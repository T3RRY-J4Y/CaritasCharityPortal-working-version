using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface IDonationService
    {
        Task<List<Donation>> GetAllDonationsAsync();
        Task<Donation?> GetDonationByIdAsync(string id);
        Task<List<Donation>> GetDonationsByEmailAsync(string email);
        Task<Donation> CreateDonationAsync(DonationCreateDto dto);
        Task<bool> UpdateDonationStatusAsync(string id, string status);
        Task<bool> DeleteDonationAsync(string id);
        Task<DonationStatsDto> GetStatsAsync();
    }

    public class DonationStatsDto
    {
        public int TotalDonations { get; set; }
        public decimal TotalMonetary { get; set; }
        public int TotalFoodParcels { get; set; }
        public int PendingReview { get; set; }
    }
}