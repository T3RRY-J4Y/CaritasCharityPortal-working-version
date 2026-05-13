using Newtonsoft.Json;

namespace CaritasCharityPortal.Models
{
    public class AssistanceRequest
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("referenceNumber")]
        public string? ReferenceNumber { get; set; }

        [JsonProperty("requestingFor")]
        public string? RequestingFor { get; set; }

        [JsonProperty("familyCount")]
        public int? FamilyCount { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("contactNumber")]
        public string? ContactNumber { get; set; }

        [JsonProperty("physicalAddress")]
        public string? PhysicalAddress { get; set; }

        [JsonProperty("churchAttended")]
        public string? ChurchAttended { get; set; }

        [JsonProperty("reasonForDonation")]
        public string? ReasonForDonation { get; set; }

        [JsonProperty("totalFamilyMembers")]
        public int TotalFamilyMembers { get; set; }

        [JsonProperty("childrenUnder18")]
        public int ChildrenUnder18 { get; set; }

        [JsonProperty("adults")]
        public int Adults { get; set; }

        [JsonProperty("membersInSchool")]
        public string? MembersInSchool { get; set; }

        [JsonProperty("membersEmployed")]
        public string? MembersEmployed { get; set; }

        [JsonProperty("employmentStatus")]
        public string? EmploymentStatus { get; set; }

        [JsonProperty("governmentGrants")]
        public string? GovernmentGrants { get; set; }

        [JsonProperty("periodAssistanceRequired")]
        public string? PeriodAssistanceRequired { get; set; }

        [JsonProperty("additionalInformation")]
        public string? AdditionalInformation { get; set; }

        [JsonProperty("isUrgent")]
        public bool IsUrgent { get; set; }

        [JsonProperty("declarationAccepted")]
        public bool DeclarationAccepted { get; set; }

        [JsonProperty("submittedAt")]
        public DateTime SubmittedAt { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("userEmail")]
        public string? UserEmail { get; set; }
    }
}