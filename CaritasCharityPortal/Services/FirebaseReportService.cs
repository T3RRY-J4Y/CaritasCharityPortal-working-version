using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text;

namespace CaritasCharityPortal.Services
{
    public class FirebaseReportService : IReportService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseReportService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl!);
        }

        public async Task<ReportViewModel> GetReportDataAsync(string reportType, string dateRange)
        {
            var (startDate, endDate) = GetDateRange(dateRange);
            var allDonations = await GetAllDonationsAsync();
            var filteredDonations = allDonations.Where(d => d.DonationDate >= startDate && d.DonationDate <= endDate).ToList();

            var volunteers = await GetAllVolunteersAsync();
            var assistanceRequests = await GetAllAssistanceRequestsAsync();
            var inventory = await GetInventoryAsync();

            var totalDonations = filteredDonations.Count;
            var totalValue = filteredDonations.Where(d => d.DonationType == "Money" && d.Amount.HasValue).Sum(d => d.Amount ?? 0);
            var uniqueDonors = filteredDonations.Select(d => d.Email).Distinct().Count();
            var itemsDistributed = filteredDonations.Where(d => d.DonationType != "Money").Sum(d => d.Quantity ?? 0);

            // Category summaries
            var categories = new[] { "Food Parcels", "Money", "Clothing", "Blankets" };
            var categorySummaries = new List<CategorySummary>();
            foreach (var cat in categories)
            {
                var catDonations = filteredDonations.Where(d => d.DonationType == cat).ToList();
                var totalCatDonations = catDonations.Count;
                var totalCatValue = catDonations.Where(d => d.Amount.HasValue).Sum(d => d.Amount ?? 0);
                var distributed = catDonations.Where(d => d.Status == "Distributed" || d.Status == "Stored").Sum(d => d.Quantity ?? 0);
                var inStock = catDonations.Where(d => d.Status == "Received" || d.Status == "Pending").Sum(d => d.Quantity ?? 0);
                var donors = catDonations.Select(d => d.Email).Distinct().Count();
                categorySummaries.Add(new CategorySummary
                {
                    Category = cat,
                    TotalDonations = totalCatDonations,
                    TotalValue = totalCatValue,
                    Distributed = distributed,
                    InStock = inStock,
                    Donors = donors
                });
            }

            // Top donors
            var donorGroups = filteredDonations
                .Where(d => !string.IsNullOrEmpty(d.DonorName))
                .GroupBy(d => new { d.DonorName, d.Email })
                .Select(g => new TopDonor
                {
                    DonorName = g.Key.DonorName,
                    DonationCount = g.Count(),
                    TotalValue = g.Where(d => d.Amount.HasValue).Sum(d => d.Amount ?? 0),
                    LastDonation = g.Max(d => d.DonationDate)
                })
                .OrderByDescending(t => t.TotalValue)
                .Take(10)
                .ToList();
            for (int i = 0; i < donorGroups.Count; i++) donorGroups[i].Rank = i + 1;

            // Monthly impact (last 6 months)
            var monthlyImpacts = new List<MonthlyImpact>();
            var now = DateTime.UtcNow;
            for (int i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthDonations = allDonations.Where(d => d.DonationDate >= monthStart && d.DonationDate <= monthEnd).ToList();
                var monthVolunteers = volunteers.Where(v => v.SubmittedAt >= monthStart && v.SubmittedAt <= monthEnd).Count();
                var monthAssistance = assistanceRequests.Where(a => a.SubmittedAt >= monthStart && a.SubmittedAt <= monthEnd).Count();
                monthlyImpacts.Add(new MonthlyImpact
                {
                    Month = monthStart.ToString("MMM yyyy"),
                    Donations = monthDonations.Count,
                    Value = monthDonations.Where(d => d.Amount.HasValue).Sum(d => d.Amount ?? 0),
                    Volunteers = monthVolunteers,
                    AssistanceRequests = monthAssistance
                });
            }

            return new ReportViewModel
            {
                TotalValue = totalValue,
                TotalDonations = totalDonations,
                UniqueDonors = uniqueDonors,
                ItemsDistributed = itemsDistributed,
                CategorySummaries = categorySummaries,
                TopDonors = donorGroups,
                MonthlyImpacts = monthlyImpacts
            };
        }

        public async Task<byte[]> ExportToCsvAsync(string reportType, string dateRange)
        {
            var (startDate, endDate) = GetDateRange(dateRange);
            var data = await GetReportDataAsync(reportType, dateRange);
            var sb = new StringBuilder();
            if (reportType == "Donation Summary")
            {
                sb.AppendLine("Category,Total Donations,Total Value,Distributed,In Stock,Donors");
                foreach (var c in data.CategorySummaries)
                    sb.AppendLine($"{c.Category},{c.TotalDonations},{c.TotalValue},{c.Distributed},{c.InStock},{c.Donors}");
            }
            else if (reportType == "Top Donors")
            {
                sb.AppendLine("Rank,Donor Name,Donation Count,Total Value,Last Donation");
                foreach (var d in data.TopDonors)
                    sb.AppendLine($"{d.Rank},{d.DonorName},{d.DonationCount},{d.TotalValue},{d.LastDonation:yyyy-MM-dd}");
            }
            else
            {
                sb.AppendLine("Month,Donations,Value,Volunteers,Assistance Requests");
                foreach (var m in data.MonthlyImpacts)
                    sb.AppendLine($"{m.Month},{m.Donations},{m.Value},{m.Volunteers},{m.AssistanceRequests}");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private (DateTime start, DateTime end) GetDateRange(string range)
        {
            var now = DateTime.UtcNow;
            return range switch
            {
                "Today" => (now.Date, now.Date),
                "This Week" => (now.Date.AddDays(-(int)now.DayOfWeek), now.Date),
                "This Month" => (new DateTime(now.Year, now.Month, 1), now.Date),
                "This Quarter" => (new DateTime(now.Year, ((now.Month - 1) / 3) * 3 + 1, 1), now.Date),
                "This Year" => (new DateTime(now.Year, 1, 1), now.Date),
                _ => (now.Date.AddMonths(-1), now.Date)
            };
        }

        // Helper methods to fetch data from Firebase
        private async Task<List<Donation>> GetAllDonationsAsync()
        {
            var snapshot = await _firebaseClient.Child("donations").OnceAsync<Donation>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }
        private async Task<List<Volunteer>> GetAllVolunteersAsync()
        {
            var snapshot = await _firebaseClient.Child("volunteers").OnceAsync<Volunteer>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }
        private async Task<List<AssistanceRequest>> GetAllAssistanceRequestsAsync()
        {
            var snapshot = await _firebaseClient.Child("assistanceRequests").OnceAsync<AssistanceRequest>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }
        private async Task<List<InventoryItem>> GetInventoryAsync()
        {
            var snapshot = await _firebaseClient.Child("inventory").OnceAsync<InventoryItem>();
            return snapshot.Select(s => { s.Object.Id = s.Key; return s.Object; }).ToList();
        }
    }
}