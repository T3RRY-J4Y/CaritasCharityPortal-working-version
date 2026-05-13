namespace CaritasCharityPortal.Models
{
    public class AssistancePageViewModel
    {
        public int Rejected { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Urgent { get; set; }
        public List<AssistanceRequest> Requests { get; set; } = new();
    }
}