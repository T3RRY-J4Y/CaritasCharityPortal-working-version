using Newtonsoft.Json;
using CaritasCharityPortal.Converters;

namespace CaritasCharityPortal.Models
{
    public class InventoryItem
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("itemCode")]
        public string? ItemCode { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("lastUpdated")]
        [JsonConverter(typeof(FlexibleDateTimeConverter))]
        public DateTime LastUpdated { get; set; }

        [JsonProperty("storageLocation")]
        public string? StorageLocation { get; set; }
    }
}