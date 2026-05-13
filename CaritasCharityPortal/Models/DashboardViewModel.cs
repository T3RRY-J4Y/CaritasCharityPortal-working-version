namespace CaritasCharityPortal.Models
{
    public class DashboardViewModel
    {
        public int TotalDonations { get; set; }
        public int AssistanceRequests { get; set; }
        public int InventoryItems { get; set; }
        public int ActiveVolunteers { get; set; }

        public List<RecentDonation> RecentDonations { get; set; } = new();
        public List<AssistanceRequestSummary> PendingAssistanceRequests { get; set; } = new();
    }

    public class RecentDonation
    {
        public string DonorName { get; set; } = "";
        public string DonationType { get; set; } = "";
        public string QuantityOrAmount { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusBadgeClass { get; set; } = "";
    }

    public class AssistanceRequestSummary
    {
        public string ReferenceNumber { get; set; } = "";
        public string AssistanceType { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsUrgent { get; set; }
    }
}