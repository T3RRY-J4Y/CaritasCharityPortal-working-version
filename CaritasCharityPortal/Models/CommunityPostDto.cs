namespace CaritasCharityPortal.Models
{
    public class CommunityPostDto
    {
        public string? RequestingFor { get; set; }
        public int? FamilyCount { get; set; }
        public string? AssistanceType { get; set; }
        public int QuantityNeeded { get; set; }
        public string? Description { get; set; }
        public string? ContactInfo { get; set; }
        public bool IsUrgent { get; set; }
    }
}