namespace CaritasCharityPortal.Models
{
    public class InventoryViewModel
    {
        public int TotalItems { get; set; }
        public int LowStockCount { get; set; }
        public int CriticalStockCount { get; set; }
        public int WarehouseCount { get; set; } = 3; 
        public List<InventoryItem> Items { get; set; } = new();
        public List<InventoryAlert> Alerts { get; set; } = new();
    }

    public class InventoryAlert
    {
        public string Type { get; set; } = ""; // "warning" or "danger"
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
    }
}