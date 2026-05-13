using Newtonsoft.Json;

namespace CaritasCharityPortal.Models
{
    public class Donation
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("reference")]
        public string? Reference { get; set; }

        [JsonProperty("donorName")]
        public string? DonorName { get; set; }

        [JsonProperty("donationType")]
        public string? DonationType { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("amount")]
        public decimal? Amount { get; set; }

        [JsonProperty("donationDate")]
        public DateTime DonationDate { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        public string DisplayQuantityOrAmount => DonationType == "Money"
            ? (Amount?.ToString("C") ?? "R0")
            : (Quantity?.ToString() ?? "0");
    }

    public class DonationCreateDto
    {
        public string? DonorName { get; set; }
        public string? DonationType { get; set; }
        public string? QuantityOrAmount { get; set; }
        public DateTime DonationDate { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
    }
}