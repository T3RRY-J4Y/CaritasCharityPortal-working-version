using Newtonsoft.Json;
using CaritasCharityPortal.Converters;

namespace CaritasCharityPortal.Models
{
    public class Receipt
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("receiptId")]
        public string? ReceiptId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("fileLink")]
        public string? FileLink { get; set; }

        [JsonProperty("purchaseDate")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime PurchaseDate { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("uploadedAt")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime UploadedAt { get; set; }
    }

    public class ReceiptCreateDto
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? FileLink { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}