namespace CaritasCharityPortal.Models
{
    public class ReportViewModel
    {
        public decimal TotalValue { get; set; }
        public int TotalDonations { get; set; }
        public int UniqueDonors { get; set; }
        public int ItemsDistributed { get; set; }

        public List<CategorySummary> CategorySummaries { get; set; } = new();
        public List<TopDonor> TopDonors { get; set; } = new();
        public List<MonthlyImpact> MonthlyImpacts { get; set; } = new();
    }

    public class CategorySummary
    {
        public string Category { get; set; } = "";
        public int TotalDonations { get; set; }
        public decimal TotalValue { get; set; }
        public int Distributed { get; set; }
        public int InStock { get; set; }
        public int Donors { get; set; }
    }

    public class TopDonor
    {
        public int Rank { get; set; }
        public string DonorName { get; set; } = "";
        public int DonationCount { get; set; }
        public decimal TotalValue { get; set; }
        public DateTime LastDonation { get; set; }
    }

    public class MonthlyImpact
    {
        public string Month { get; set; } = "";
        public int Donations { get; set; }
        public decimal Value { get; set; }
        public int Volunteers { get; set; }
        public int AssistanceRequests { get; set; }
    }
}