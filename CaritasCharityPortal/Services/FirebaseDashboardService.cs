using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace CaritasCharityPortal.Services
{
    public class FirebaseDashboardService : IDashboardService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseDashboardService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl!);
        }

        private async Task<List<Donation>> GetAllDonationsAsync()
        {
            var snapshot = await _firebaseClient.Child("donations").OnceAsync<Donation>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }

        private async Task<List<AssistanceRequest>> GetAllAssistanceRequestsAsync()
        {
            var snapshot = await _firebaseClient.Child("assistanceRequests").OnceAsync<AssistanceRequest>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }

        private async Task<List<InventoryItem>> GetAllInventoryAsync()
        {
            var snapshot = await _firebaseClient.Child("inventory").OnceAsync<InventoryItem>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }

        private async Task<List<Volunteer>> GetAllVolunteersAsync()
        {
            var snapshot = await _firebaseClient.Child("volunteers").OnceAsync<Volunteer>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }

        private string GetBadgeClass(string? status)
        {
            return status switch
            {
                "Received" => "blue",
                "Stored" => "green",
                "Sorted" => "yellow",
                "Approved" => "green",
                "Rejected" => "red",
                "Pending" => "yellow",
                _ => "gray"
            };
        }

        public async Task<DashboardViewModel> GetAdminDashboardAsync()
        {
            var donations = await GetAllDonationsAsync();
            var assistance = await GetAllAssistanceRequestsAsync();
            var inventory = await GetAllInventoryAsync();
            var volunteers = await GetAllVolunteersAsync();

            var totalDonations = donations.Count;
            var totalAssistance = assistance.Count;
            var totalInventory = inventory.Sum(i => i.Quantity);
            var activeVolunteers = volunteers.Count(v => v.Status == "Active");

            var recentDonations = donations
                .OrderByDescending(d => d.DonationDate)
                .Take(5)
                .Select(d => new RecentDonation
                {
                    DonorName = d.DonorName ?? "Anonymous",
                    DonationType = d.DonationType ?? "",
                    QuantityOrAmount = d.DisplayQuantityOrAmount,
                    Status = d.Status ?? "Pending",
                    StatusBadgeClass = GetBadgeClass(d.Status)
                }).ToList();

            // Use ReasonForDonation as the "AssistanceType" (fallback)
            var pendingAssistance = assistance
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.SubmittedAt)
                .Take(5)
                .Select(a => new AssistanceRequestSummary
                {
                    ReferenceNumber = a.ReferenceNumber ?? "",
                    AssistanceType = a.ReasonForDonation?.Length > 30 ? a.ReasonForDonation.Substring(0, 30) + "..." : (a.ReasonForDonation ?? "General"),
                    Status = a.Status ?? "Pending",
                    IsUrgent = a.IsUrgent
                }).ToList();

            return new DashboardViewModel
            {
                TotalDonations = totalDonations,
                AssistanceRequests = totalAssistance,
                InventoryItems = totalInventory,
                ActiveVolunteers = activeVolunteers,
                RecentDonations = recentDonations,
                PendingAssistanceRequests = pendingAssistance
            };
        }

        public async Task<DashboardViewModel> GetUserDashboardAsync(string? userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
                return new DashboardViewModel();

            var allDonations = await GetAllDonationsAsync();
            var userDonations = allDonations.Where(d => d.Email == userEmail).ToList();

            var allAssistance = await GetAllAssistanceRequestsAsync();
            var userAssistance = allAssistance.Where(a => a.UserEmail == userEmail).ToList();

            var recentDonations = userDonations
                .OrderByDescending(d => d.DonationDate)
                .Take(5)
                .Select(d => new RecentDonation
                {
                    DonorName = d.DonorName ?? "Anonymous",
                    DonationType = d.DonationType ?? "",
                    QuantityOrAmount = d.DisplayQuantityOrAmount,
                    Status = d.Status ?? "Pending",
                    StatusBadgeClass = GetBadgeClass(d.Status)
                }).ToList();

            var pendingAssistance = userAssistance
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.SubmittedAt)
                .Take(5)
                .Select(a => new AssistanceRequestSummary
                {
                    ReferenceNumber = a.ReferenceNumber ?? "",
                    AssistanceType = a.ReasonForDonation?.Length > 30 ? a.ReasonForDonation.Substring(0, 30) + "..." : (a.ReasonForDonation ?? "General"),
                    Status = a.Status ?? "Pending",
                    IsUrgent = a.IsUrgent
                }).ToList();

            return new DashboardViewModel
            {
                TotalDonations = userDonations.Count,
                AssistanceRequests = userAssistance.Count,
                InventoryItems = 0,
                ActiveVolunteers = 0,
                RecentDonations = recentDonations,
                PendingAssistanceRequests = pendingAssistance
            };
        }
    }
}