using Newtonsoft.Json;
using CaritasCharityPortal.Converters;

namespace CaritasCharityPortal.Models
{
    public class Volunteer
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("applicationId")]
        public string? ApplicationId { get; set; }

        [JsonProperty("fullName")]
        public string? FullName { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("phone")]
        public string? Phone { get; set; }

        [JsonProperty("location")]
        public string? Location { get; set; }

        [JsonProperty("skills")]
        public string? Skills { get; set; }

        [JsonProperty("availability")]
        public string? Availability { get; set; }

        [JsonProperty("motivation")]
        public string? Motivation { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("totalHours")]
        public int TotalHours { get; set; }

        [JsonProperty("submittedAt")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime SubmittedAt { get; set; }
    }

    public class VolunteerCreateDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string? Skills { get; set; }
        public string? Availability { get; set; }
        public string? Motivation { get; set; }
    }
}