using CaritasCharityPortal.Models;

namespace CaritasCharityPortal.Services
{
    public interface IInventoryService
    {
        Task<InventoryViewModel> GetInventoryDataAsync();
        Task<List<InventoryItem>> GetAllItemsAsync();
        Task<InventoryItem?> GetItemByIdAsync(string id);
        Task<InventoryItem> AddStockAsync(InventoryItem item);
        Task<InventoryItem> RemoveStockAsync(string id, int quantityToRemove);
        Task<bool> DeleteItemAsync(string id);
    }
}