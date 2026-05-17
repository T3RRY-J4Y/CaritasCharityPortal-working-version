using Newtonsoft.Json;
using CaritasCharityPortal.Converters;

namespace CaritasCharityPortal.Models
{
    public class CommunityPost
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("postId")]
        public string? PostId { get; set; }

        [JsonProperty("requestingFor")]
        public string? RequestingFor { get; set; } // "Individual" or "Family"

        [JsonProperty("familyCount")]
        public int? FamilyCount { get; set; }

        [JsonProperty("assistanceType")]
        public string? AssistanceType { get; set; } // "Food Parcels", "Clothing", etc.

        [JsonProperty("quantityNeeded")]
        public int QuantityNeeded { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("contactInfo")]
        public string? ContactInfo { get; set; } // optional, visible only to admin

        [JsonProperty("isUrgent")]
        public bool IsUrgent { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; } // "Open", "Partially Fulfilled", "Fulfilled"

        [JsonProperty("userEmail")]
        public string? UserEmail { get; set; } // for tracking, not displayed

        [JsonProperty("createdAt")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("responseCount")]
        public int ResponseCount { get; set; } // number of offers/responses (placeholder)
    }
}