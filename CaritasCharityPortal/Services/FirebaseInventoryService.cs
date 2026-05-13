using CaritasCharityPortal.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace CaritasCharityPortal.Services
{
    public class FirebaseInventoryService : IInventoryService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string Node = "inventory";

        public FirebaseInventoryService(IConfiguration configuration)
        {
            var databaseUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(databaseUrl!);
        }

        public async Task<List<InventoryItem>> GetAllItemsAsync()
        {
            var snapshot = await _firebaseClient
                .Child(Node)
                .OrderBy("itemCode")
                .OnceAsync<InventoryItem>();

            return snapshot.Select(item =>
            {
                item.Object.Id = item.Key;
                return item.Object;
            }).ToList();
        }

        public async Task<InventoryViewModel> GetInventoryDataAsync()
        {
            var items = await GetAllItemsAsync();
            var totalItems = items.Sum(i => i.Quantity);
            var lowStock = items.Count(i => i.Quantity <= 20 && i.Quantity > 5);
            var critical = items.Count(i => i.Quantity <= 5);

            var alerts = new List<InventoryAlert>();

            // Low stock alerts (max 1)
            var lowStockItems = items.Where(i => i.Quantity <= 20 && i.Quantity > 5).Take(1);
            foreach (var item in lowStockItems)
            {
                alerts.Add(new InventoryAlert
                {
                    Type = "warning",
                    Title = "Low Stock Alert",
                    Message = $"{item.Description} are running low. Current stock: {item.Quantity} units"
                });
            }

            // Critical stock alerts (max 1)
            var criticalItems = items.Where(i => i.Quantity <= 5).Take(1);
            foreach (var item in criticalItems)
            {
                alerts.Add(new InventoryAlert
                {
                    Type = "danger",
                    Title = "Critical Stock Alert",
                    Message = $"{item.Description} are critically low. Immediate restock needed!"
                });
            }

            return new InventoryViewModel
            {
                TotalItems = totalItems,
                LowStockCount = lowStock,
                CriticalStockCount = critical,
                WarehouseCount = 3, // can be made dynamic later
                Items = items,
                Alerts = alerts
            };
        }

        public async Task<InventoryItem?> GetItemByIdAsync(string id)
        {
            return await _firebaseClient.Child(Node).Child(id).OnceSingleAsync<InventoryItem>();
        }

        public async Task<InventoryItem> AddStockAsync(InventoryItem item)
        {
            item.ItemCode = GenerateItemCode();
            item.LastUpdated = DateTime.UtcNow;

            var result = await _firebaseClient.Child(Node).PostAsync(item);
            item.Id = result.Key;
            return item;
        }

        public async Task<InventoryItem> RemoveStockAsync(string id, int quantityToRemove)
        {
            var item = await GetItemByIdAsync(id);
            if (item == null) throw new InvalidOperationException("Item not found.");

            if (item.Quantity < quantityToRemove)
                throw new InvalidOperationException($"Cannot remove {quantityToRemove} units. Only {item.Quantity} in stock.");

            item.Quantity -= quantityToRemove;
            item.LastUpdated = DateTime.UtcNow;

            await _firebaseClient.Child(Node).Child(id).PatchAsync(item);
            return item;
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            await _firebaseClient.Child(Node).Child(id).DeleteAsync();
            return true;
        }

        private string GenerateItemCode()
        {
            return $"INV-{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}