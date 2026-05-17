namespace CaritasCharityPortal.Models
{
    public class CommunityViewModel
    {
        public int TotalPosts { get; set; }
        public int TotalResponses { get; set; }
        public int ActiveRequests { get; set; }
        public int UrgentPosts { get; set; }
        public List<CommunityPost> Posts { get; set; } = new();
    }
}